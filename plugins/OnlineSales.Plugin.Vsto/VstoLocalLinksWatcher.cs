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
        this.VstoLocalPath = vstoLocalPath;
        this.services = services;
        this.VstoRequestPath = vstoRequestPath;
        this.Init();

        if (Directory.Exists(vstoLocalPath))
        {
            this.exeWatcher = new FileSystemWatcher(vstoLocalPath);
            this.exeWatcher.NotifyFilter = NotifyFilters.FileName;
            this.exeWatcher.IncludeSubdirectories = true;
            this.exeWatcher.Filter = "*.exe";
            this.exeWatcher.Created += this.HandleChanged;
            this.exeWatcher.Deleted += this.HandleChanged;
            this.exeWatcher.Renamed += this.HandleRenamed;

            this.exeWatcher.EnableRaisingEvents = true;
        }
        else
        {
            Log.Warning($"The folder references by VstoLocalPath setting does not exist: {vstoLocalPath}");
        }
    }

    public void Dispose()
    {
        if (this.exeWatcher != null)
        {
            this.exeWatcher.Dispose();
        }
    }

    private void HandleRenamed(object sender, RenamedEventArgs e)
    {
        var ed = DictionaryExtensions.TryGetAndReturn(this.exeDirs, e.OldFullPath);
        if (ed != null)
        {
            this.exeDirs.Remove(e.OldFullPath);
            this.exeDirs.Add(e.FullPath, ed);
        }
    }

    private void HandleChanged(object sender, FileSystemEventArgs e)
    {
        if (e.ChangeType == WatcherChangeTypes.Created)
        {
            var parentDir = Directory.GetParent(e.FullPath);
            if (parentDir != null)
            {
                this.exeDirs.Add(e.FullPath, new ExeDirectory(this, parentDir));
            }
        }
        else if (e.ChangeType == WatcherChangeTypes.Deleted)
        {
            var ed = DictionaryExtensions.TryGetAndReturn(this.exeDirs, e.FullPath);
            if (ed != null)
            {
                ed.StopAndClear();
                this.exeDirs.Remove(e.FullPath);
            }
        }
    }

    private void Init()
    {
        var exeFiles = Array.Empty<string>();
        if (Directory.Exists(this.VstoLocalPath))
        {
            exeFiles = Directory.GetFiles(this.VstoLocalPath, "*.exe", SearchOption.AllDirectories);
        }

        var allLinks = new HashSet<OnlineSales.Entities.Link>(new LinkComparer());
        foreach (var exeFile in exeFiles)
        {
            var parentDir = Directory.GetParent(exeFile);
            if (parentDir != null)
            {
                var exeDir = new ExeDirectory(this, parentDir);
                this.exeDirs.Add(exeFile, exeDir);
                var links = exeDir.GetLinks();
                allLinks.UnionWith(links);
            }
        }

        using (var serviceProvider = this.services!.BuildServiceProvider())
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
        using (var serviceProvider = this.services!.BuildServiceProvider())
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
        using (var serviceProvider = this.services!.BuildServiceProvider())
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
            this.parenDir = di;

            this.CheckAndInit();

            this.links = this.CreateLinks();

            this.linksWatcher.AddLinks(this.links);

            this.watcher = new FileSystemWatcher(this.parenDir.FullName);
            this.watcher.NotifyFilter = NotifyFilters.DirectoryName | NotifyFilters.FileName;
            this.watcher.Filter = "*";
            this.watcher.IncludeSubdirectories = true;

            this.watcher.Created += this.HandleChanged;
            this.watcher.Deleted += this.HandleChanged;
            this.watcher.Renamed += this.HandleChanged;

            this.watcher.EnableRaisingEvents = true;
        }

        public HashSet<OnlineSales.Entities.Link> GetLinks()
        {
            return this.links;
        }

        public void StopAndClear()
        {
            this.linksMutex.WaitOne();
            try
            {
                this.watcher.EnableRaisingEvents = false;
                this.linksWatcher.RemoveLinks(this.links);
            }
            finally
            {
                this.linksMutex.ReleaseMutex();
            }
        }

        private HashSet<OnlineSales.Entities.Link> CreateLinks()
        {
            var result = new HashSet<OnlineSales.Entities.Link>(new LinkComparer());

            if (this.valid)
            {
                var resourceName = Path.GetFileNameWithoutExtension(this.vstoFile!.Name) + "_";
                var relPath = Path.GetRelativePath(this.linksWatcher.VstoLocalPath, this.parenDir.FullName).Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                var versionDirs = this.GetVersionDirs(resourceName);
                foreach (var versionDir in versionDirs)
                {
                    if (versionDir != null)
                    {
                        var version = versionDir.Name.Substring(resourceName.Length).Replace('_', '.');
                        var name = relPath.Replace(Path.AltDirectorySeparatorChar, '_') + "_" + version;

                        result.Add(new OnlineSales.Entities.Link
                        {
                            Uid = name,
                            Destination = Path.Combine(this.linksWatcher.VstoRequestPath, relPath, this.exeFile!.Name).Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + "?=" + version,
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
            if (this.appDir != null)
            {
                try
                {
                    return this.appDir!.GetDirectories(resourceName + "*");
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
            this.linksMutex.WaitOne();
            try
            {
                if (this.valid)
                {
                    this.CheckAndInit();
                    if (!this.valid)
                    {
                        this.linksWatcher.RemoveLinks(this.links);
                        this.links.Clear();
                    }
                    else
                    {
                        var newLinks = this.CreateLinks();
                        if (!this.links.SetEquals(newLinks))
                        {
                            this.linksWatcher.RemoveLinks(this.links);
                            this.links = newLinks;
                            this.linksWatcher.AddLinks(this.links);
                        }
                    }
                }
                else
                {
                    this.CheckAndInit();
                    if (this.valid)
                    {
                        this.links = this.CreateLinks();
                        this.linksWatcher.AddLinks(this.links);
                    }
                }
            }
            finally
            {
                this.linksMutex.ReleaseMutex();
            }
        }

        private bool CheckAndInitExeFile()
        {
            var exeFiles = this.parenDir.GetFiles("*.exe", SearchOption.AllDirectories);
            if (exeFiles.Length == 1)
            {
                this.exeFile = exeFiles[0];
                return true;
            }

            return false;
        }

        private bool CheckAndInitVstoFile()
        {
            var vstoFiles = this.parenDir.GetFiles("*.vsto", SearchOption.TopDirectoryOnly);
            if (vstoFiles.Length == 1)
            {
                this.vstoFile = vstoFiles[0];
                return true;
            }

            return false;
        }

        private bool CheckAndInitAppDir()
        {
            var appDirs = this.parenDir.GetDirectories("Application Files");
            if (appDirs.Length == 1)
            {
                this.appDir = appDirs[0];
                return true;
            }

            return false;
        }

        private void CheckAndInit()
        {
            this.valid = this.CheckAndInitExeFile() && this.CheckAndInitVstoFile() && this.CheckAndInitAppDir();
        }
    }
}