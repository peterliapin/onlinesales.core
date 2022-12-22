// <copyright file="VstoFileProvider.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using OnlineSales.Data;
using OnlineSales.Plugin.Vsto.Data;
using OnlineSales.Plugin.Vsto.Entities;

namespace OnlineSales.Plugin.Vsto;

public sealed class VstoFileProvider : IFileProvider
{
    private readonly string vstoRootPath;
    private readonly IHttpContextHelper httpContextHelper;
    private readonly IServiceCollection services;

    public VstoFileProvider(string vstoRootPath, IHttpContextHelper httpContextHelper, IServiceCollection services)
    {
        this.vstoRootPath = vstoRootPath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
        this.httpContextHelper = httpContextHelper;
        this.services = services;
    }

    private enum VstoFileType
    {
        None,
        Exe,
        Vsto,
    }

    public IDirectoryContents GetDirectoryContents(string subpath)
    {
        throw new NotImplementedException();
    }

    public IFileInfo GetFileInfo(string subpath)
    {
        var result = new VstoFileInfo(
            vstoRootPath,
            subpath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar));

        if (ParseContext(out var fileType, out var ipAddress, out var version, out var subfolder, subpath))
        {
            using (var serviceProvider = services!.BuildServiceProvider())
            {
                using (var scope = serviceProvider.CreateScope())
                {
                    var db = PluginDbContextBase.GetPluginDbContext<VstoDbContext>(scope);

                    if (fileType == VstoFileType.Exe)
                    {
                        HandleExeRequest(ipAddress, version, subfolder, db!);
                    }
                    else if (fileType == VstoFileType.Vsto)
                    {
                        HandleManifestRequest(ipAddress, subpath, db!, ref result);
                    }
                }
            }
        }

        return result.Exists ? result as IFileInfo : new NotFoundFileInfo(subpath);
    }

    public IChangeToken Watch(string filter)
    {
        throw new NotImplementedException();
    }

    private bool ParseContext(out VstoFileType fileType, out string ipAddress, out string version, out string subfolder, string subpath)
    {
        if (!Enum.TryParse(System.IO.Path.GetExtension(subpath).Replace(".", string.Empty), true, out fileType))
        {
            fileType = VstoFileType.None;
        }

        ipAddress = httpContextHelper.IpAddress!;
        version = string.Empty;
        subfolder = string.Empty;

        if (fileType == VstoFileType.Exe)
        {
            // 'subpath' may starts from '/'. We do not need it in subfolder.
            subfolder = Path.GetDirectoryName(subpath) ?? string.Empty;
            if (subfolder[0] == Path.DirectorySeparatorChar)
            {
                subfolder = subfolder.Substring(1);
            }

            var request = httpContextHelper.Request;

            string[] verQuery = { "version", "ver", "v" };
            foreach (var verVariant in verQuery)
            {
                StringValues? readVer = request.Query[verVariant];
                if (!string.IsNullOrEmpty(readVer))
                {
                    version = readVer!;
                    break;
                }
            }
        }

        return fileType != VstoFileType.None;
    }

    private void HandleExeRequest(string ipAddress, string version, string subdir, VstoDbContext db)
    {
        if (!string.IsNullOrEmpty(ipAddress))
        {
            // let's forget if we have already provided any specified version to this client
            var toDelete = db.VstoUserVersions!.Where(r => r.IpAddress == ipAddress).ToList();
            if (toDelete.Any())
            {
                toDelete.ForEach(r => db.VstoUserVersions!.Remove(r));
                db.SaveChanges();
            }

            // let's remember the required version if it is specified
            if (!string.IsNullOrEmpty(version) || !string.IsNullOrEmpty(subdir))
            {
                db.VstoUserVersions!.Add(new Entities.VstoUserVersion
                {
                    IpAddress = ipAddress,
                    Version = version,
                    ExpireDateTime = DateTime.UtcNow.AddDays(1),
                    Subfolder = subdir,
                });
                db.SaveChanges();
            }
        }
    }

    private void HandleManifestRequest(string ipAddress, string subpath, VstoDbContext db, ref VstoFileInfo result)
    {
        var stat = db.VstoUserVersions!.Where(r => r.IpAddress == ipAddress).FirstOrDefault();
        if (stat != null)
        {
            var manifestPath = stat.Subfolder;
            if (!string.IsNullOrEmpty(stat.Version) && stat.ExpireDateTime > DateTime.Now)
            {
                manifestPath = Path.Join(
                    manifestPath,
                    Path.Join(
                        "Application Files",
                        Path.GetFileNameWithoutExtension(subpath) + "_" + stat.Version.Replace('.', '_')));
            }

            manifestPath = Path.Join(manifestPath, subpath);
            result = new VstoFileInfo(vstoRootPath, manifestPath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar));
        }
    }
}