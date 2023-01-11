// <copyright file="VstoLocalLinksWatcher.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using Microsoft.Extensions.DependencyInjection;
using OnlineSales.Data;
using OnlineSales.Plugin.Vsto.Data;

namespace OnlineSales.Plugin.Vsto;

public class VstoLocalLinksWatcher
{
    private readonly string vstoLocalPath;

    private readonly string vstoRequestPath;

    private readonly IServiceCollection services;

    #pragma warning disable S1450  
    private readonly FileSystemWatcher watcher;
    #pragma warning restore S1450

    public VstoLocalLinksWatcher(string vstoLocalPath, string vstoRequestPath, IServiceCollection services)
    {
        this.vstoLocalPath = vstoLocalPath;
        this.services = services;
        this.vstoRequestPath = vstoRequestPath;

        InitLinks();

        watcher = new FileSystemWatcher(vstoLocalPath);
        watcher.NotifyFilter = NotifyFilters.DirectoryName | NotifyFilters.FileName;
        watcher.Filter = "*";
        watcher.Created += HandleChanged;
        watcher.Deleted += HandleChanged;
        watcher.Renamed += HandleChanged;

        watcher.EnableRaisingEvents = true;
    }

    private void HandleChanged(object sender, FileSystemEventArgs e)
    {
        InitLinks();
    }

    private void InitLinks()
    {
        using (var serviceProvider = services!.BuildServiceProvider())
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var dbContext = PluginDbContextBase.GetPluginDbContext<VstoDbContext>(scope);
                if (dbContext.Links != null)
                {
                    foreach (var link in dbContext.Links)
                    {
                        dbContext.Links.Remove(link);
                    }

                    var exeFiles = Directory.GetFiles(vstoLocalPath, "*.exe", SearchOption.AllDirectories);
                    foreach (var exeFile in exeFiles)
                    {
                        var parentDir = Directory.GetParent(exeFile);
                        // we expect the only exe file
                        if (parentDir != null && parentDir.GetFiles("*.exe").Length == 1)
                        {
                            var vstoFiles = parentDir.GetFiles("*.vsto");
                            // we expect the only vsto file
                            if (vstoFiles.Length == 1)
                            {
                                var vstoFile = vstoFiles[0];
                                var resourceName = Path.GetFileNameWithoutExtension(vstoFile.Name) + "_";
                                var relPath = Path.GetRelativePath(vstoLocalPath, parentDir.FullName).Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                                var appFileDirs = parentDir.GetDirectories("Application Files");
                                if (appFileDirs.Length == 1)
                                {
                                    var appFileDir = appFileDirs[0];
                                    var versionDirs = appFileDir.GetDirectories(resourceName + "*");
                                    foreach (var versionDir in versionDirs)
                                    {
                                        var version = versionDir.Name.Substring(resourceName.Length).Replace('_', '.');
                                        var name = relPath.Replace(Path.AltDirectorySeparatorChar, '_') + "_" + version;

                                        dbContext.Links.Add(new OnlineSales.Entities.Link
                                        {
                                            Uid = name,
                                            Destination = Path.Combine(vstoRequestPath, relPath, Path.GetFileName(exeFile)).Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + "?=" + version,
                                            Name = name,
                                        });
                                    }
                                }
                            }
                        }
                    }

                    dbContext.SaveChanges();
                }
            }
        }
    }
}