// <copyright file="VstoLocalLinksWatcher.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OnlineSales.Data;
using OnlineSales.Plugin.Vsto.Data;
using Quartz.Util;
using Serilog;

namespace OnlineSales.Plugin.Vsto;

public class VstoLocalLinksWatcher : IDisposable
{
    public readonly string VstoLocalPath;

    public readonly string VstoRequestPath;

    private readonly IServiceCollection services;

    private readonly FileSystemWatcher? exeWatcher;

    private readonly Dictionary<string, ExeDirectory> exeDirs = new Dictionary<string, ExeDirectory>();

    public VstoLocalLinksWatcher(string vstoLocalPath, string vstoRequestPath, IServiceCollection services)
    {
        VstoLocalPath = vstoLocalPath;
        this.services = services;
        VstoRequestPath = vstoRequestPath;
        Init();

        if (Directory.Exists(vstoLocalPath))
        {
            exeWatcher = new FileSystemWatcher(vstoLocalPath);
            exeWatcher.NotifyFilter = NotifyFilters.FileName;
            exeWatcher.IncludeSubdirectories = true;
            exeWatcher.Filter = "*.exe";
            exeWatcher.Created += HandleChanged;
            exeWatcher.Deleted += HandleChanged;
            exeWatcher.Renamed += HandleRenamed;

            exeWatcher.EnableRaisingEvents = true;
        }
        else
        {
            Log.Warning($"The folder references by VstoLocalPath setting does not exist: {vstoLocalPath}");
        }
    }

    public void Dispose()
    {
        if (exeWatcher != null)
        {
            exeWatcher.Dispose();
        }        
    }

    private void HandleRenamed(object sender, RenamedEventArgs e)
    {
        var ed = DictionaryExtensions.TryGetAndReturn(exeDirs, e.OldFullPath);
        if (ed != null)
        {            
            exeDirs.Remove(e.OldFullPath);
            exeDirs.Add(e.FullPath, ed);
        }
    }

    private void HandleChanged(object sender, FileSystemEventArgs e)
    {
        if (e.ChangeType == WatcherChangeTypes.Created)
        {
            var parentDir = Directory.GetParent(e.FullPath);
            if (parentDir != null)
            {
                exeDirs.Add(e.FullPath, new ExeDirectory(this, parentDir));
            }
        }
        else if (e.ChangeType == WatcherChangeTypes.Deleted)
        {
            var ed = DictionaryExtensions.TryGetAndReturn(exeDirs, e.FullPath);
            if (ed != null)
            {
                ed.StopAndClear();
                exeDirs.Remove(e.FullPath);
            }
        }
    }

    private void Init()
    {
        var exeFiles = Array.Empty<string>();
        if (Directory.Exists(VstoLocalPath))
        {
            exeFiles = Directory.GetFiles(VstoLocalPath, "*.exe", SearchOption.AllDirectories);
        }

        var allLinks = new HashSet<OnlineSales.Entities.Link>(new LinkComparer());
        foreach (var exeFile in exeFiles)
        {            
            var parentDir = Directory.GetParent(exeFile);
            if (parentDir != null)
            {
                var exeDir = new ExeDirectory(this, parentDir);
                exeDirs.Add(exeFile, exeDir);
                var links = exeDir.GetLinks();
                allLinks.UnionWith(links);
            }
        }

        using (var serviceProvider = services!.BuildServiceProvider())
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var dbContext = PluginDbContextBase.GetPluginDbContext<VstoDbContext>(scope);
                if (dbContext.Links != null)
                {
                    foreach (var link in dbContext.Links)
                    {
                        if (!allLinks.Contains(link, new LinkComparer()))
                        {
                            dbContext.Links.Remove(link);
                        }
                    }

                    dbContext.SaveChangesAsync().Wait();
                }                
            }
        }
    }

    private void RemoveLinks(HashSet<OnlineSales.Entities.Link> removedLinks)
    {
        using (var serviceProvider = services!.BuildServiceProvider())
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var dbContext = PluginDbContextBase.GetPluginDbContext<VstoDbContext>(scope);
                if (dbContext.Links != null)
                {
                    foreach (var link in removedLinks)
                    {
                        dbContext.Remove(dbContext.Links.Single(l => l.Uid == link.Uid));
                    }

                    dbContext.SaveChangesAsync().Wait();
                }
            }
        }
    }

    private void AddLinks(HashSet<OnlineSales.Entities.Link> addedLinks)
    {
        using (var serviceProvider = services!.BuildServiceProvider())
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var dbContext = PluginDbContextBase.GetPluginDbContext<VstoDbContext>(scope);
                if (dbContext.Links != null)
                {
                    foreach (var link in addedLinks)
                    {
                        if (!dbContext.Links.Any(l => l.Uid == link.Uid))
                        {
                            dbContext.Links.Add(link);
                        }
                    }

                    dbContext.SaveChangesAsync().Wait();
                }
            }
        }
    }

    private sealed class LinkComparer : IEqualityComparer<OnlineSales.Entities.Link>
    {
        public bool Equals(OnlineSales.Entities.Link? l1, OnlineSales.Entities.Link? l2)
        {
            if (l1 == null && l2 == null)
            {
                return true;
            }
            else if (l1 == null || l2 == null)
            {
                return false;
            }
            else
            {
                return l1.Uid == l2.Uid;
            }            
        }

        public int GetHashCode(OnlineSales.Entities.Link obj)
        {
            return obj.Uid.GetHashCode();
        }
    }

    private sealed class ExeDirectory
    {
        private readonly VstoLocalLinksWatcher linksWatcher;

        private readonly FileSystemWatcher watcher;

        private readonly Mutex linksMutex = new Mutex();

        private readonly DirectoryInfo parenDir;

        private HashSet<OnlineSales.Entities.Link> links;

        private bool valid = false;

        private FileInfo? exeFile;

        private FileInfo? vstoFile;

        private DirectoryInfo? appDir;               

        public ExeDirectory(VstoLocalLinksWatcher linksWatcher, DirectoryInfo di)
        {
            this.linksWatcher = linksWatcher;
            parenDir = di;

            CheckAndInit();

            links = CreateLinks();

            this.linksWatcher.AddLinks(links);

            watcher = new FileSystemWatcher(parenDir.FullName);
            watcher.NotifyFilter = NotifyFilters.DirectoryName | NotifyFilters.FileName;
            watcher.Filter = "*";
            watcher.IncludeSubdirectories = true;

            watcher.Created += HandleChanged;
            watcher.Deleted += HandleChanged;
            watcher.Renamed += HandleChanged;

            watcher.EnableRaisingEvents = true;
        }

        public HashSet<OnlineSales.Entities.Link> GetLinks()
        {
            return links;
        }

        public void StopAndClear()
        {
            linksMutex.WaitOne();
            try
            {
                watcher.EnableRaisingEvents = false;
                linksWatcher.RemoveLinks(links);
            }
            finally
            {
                linksMutex.ReleaseMutex();  
            }
        }

        private HashSet<OnlineSales.Entities.Link> CreateLinks()
        {
            HashSet<OnlineSales.Entities.Link> result = new HashSet<OnlineSales.Entities.Link>(new LinkComparer());

            if (valid)
            {
                var resourceName = Path.GetFileNameWithoutExtension(vstoFile!.Name) + "_";
                var relPath = Path.GetRelativePath(linksWatcher.VstoLocalPath, parenDir.FullName).Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                var versionDirs = GetVersionDirs(resourceName);
                foreach (var versionDir in versionDirs)
                {
                    if (versionDir != null)
                    {
                        var version = versionDir.Name.Substring(resourceName.Length).Replace('_', '.');
                        var name = relPath.Replace(Path.AltDirectorySeparatorChar, '_') + "_" + version;

                        result.Add(new OnlineSales.Entities.Link
                        {
                            Uid = name,
                            Destination = Path.Combine(linksWatcher.VstoRequestPath, relPath, exeFile!.Name).Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + "?=" + version,
                            Name = name,
                        });
                    }
                }
            }

            return result;
        }

        private DirectoryInfo[] GetVersionDirs(string resourceName)
        {
            var result = new DirectoryInfo[0];
            if (appDir != null)
            {
                try
                {
                    return appDir!.GetDirectories(resourceName + "*");
                }
                catch
                {
                    // do nothing
                }
            }

            return result;
        }

        private void HandleChanged(object sender, FileSystemEventArgs e)
        {
            linksMutex.WaitOne();
            try
            {
                if (valid)
                {
                    CheckAndInit();
                    if (!valid)
                    {
                        linksWatcher.RemoveLinks(links);
                        links.Clear();
                    }
                    else
                    {
                        var newLinks = CreateLinks();
                        if (!links.SetEquals(newLinks))
                        {
                            linksWatcher.RemoveLinks(links);
                            links = newLinks;
                            linksWatcher.AddLinks(links);
                        }
                    }
                }
                else
                {
                    CheckAndInit();
                    if (valid)
                    {
                        links = CreateLinks();
                        linksWatcher.AddLinks(links);
                    }
                }
            }
            finally
            {
                linksMutex.ReleaseMutex();
            }
        }               

        private bool CheckAndInitExeFile()
        {
            var exeFiles = parenDir.GetFiles("*.exe", SearchOption.AllDirectories);
            if (exeFiles.Length == 1)
            {
                exeFile = exeFiles[0];
                return true;
            }

            return false;
        }

        private bool CheckAndInitVstoFile()
        {
            var vstoFiles = parenDir.GetFiles("*.vsto", SearchOption.TopDirectoryOnly);
            if (vstoFiles.Length == 1)
            {
                vstoFile = vstoFiles[0];
                return true;
            }

            return false;
        }

        private bool CheckAndInitAppDir()
        {
            var appDirs = parenDir.GetDirectories("Application Files");
            if (appDirs.Length == 1)
            {
                appDir = appDirs[0];
                return true;
            }

            return false;
        }

        private void CheckAndInit()
        {
            valid = CheckAndInitExeFile() && CheckAndInitVstoFile() && CheckAndInitAppDir();
        }
    }
}