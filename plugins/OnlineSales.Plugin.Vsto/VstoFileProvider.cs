// <copyright file="VstoFileProvider.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

using OnlineSales.Data;
using OnlineSales.Plugin.Vsto.Data;

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
        // Fix Unix CaseSensetive mode ---------------------------
        var targetPath = Path.Combine(vstoRootPath, Path.GetDirectoryName(subpath)![1..]);
        var targetName = Path.GetFileName(subpath);
        var validFilesByExtension = Directory.GetFiles(targetPath, $"*{Path.GetExtension(subpath)}", SearchOption.AllDirectories).ToArray();
        var fileFromDirectory = validFilesByExtension.FirstOrDefault(f => f.ToLower().EndsWith(targetName.ToLower()));

        if (string.IsNullOrEmpty(fileFromDirectory))
        {
            return new NotFoundFileInfo(subpath);
        }
        else
        {
            fileFromDirectory = fileFromDirectory[vstoRootPath.Length..];
            subpath = fileFromDirectory.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }

        // -------------------------------------------------------

        var result = new VstoFileInfo(
            vstoRootPath,
            subpath);

        if (ParseContext(out var fileType, out var ipAddress, out var version, out var subfolder, subpath))
        {
            using var serviceProvider = services!.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
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

        return result.Exists ? result : new NotFoundFileInfo(subpath);
    }

    public IChangeToken Watch(string filter)
    {
        throw new NotImplementedException();
    }

    private bool ParseContext(out VstoFileType fileType, out string ipAddress, out string version, out string subfolder, string subpath)
    {
        if (!Enum.TryParse(Path.GetExtension(subpath).Replace(".", string.Empty), true, out fileType))
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
                subfolder = subfolder[1..];
            }

            var request = httpContextHelper.Request;

            string[] verQuery = { "version", "ver", "v" };
            version = request.Query.FirstOrDefault(q => verQuery.Contains(q.Key)).Value.ToString() ?? string.Empty;

            // find version if link looks like "/vsto/pro/en/XLTools.exe?=5.8.0.21596"
            if ((request.QueryString.Value?.StartsWith("?=") ?? false) && request.Query[string.Empty].FirstOrDefault()?.Length > 0) 
            {
                var verVal = request.Query[string.Empty].FirstOrDefault();
                if (Version.TryParse(verVal, out var parsed))
                {
                    version = parsed.ToString(); // tested - OK
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
            if (!string.IsNullOrEmpty(stat.Version) && stat.ExpireDateTime > DateTime.UtcNow)
            {
                manifestPath = Path.Join(manifestPath, Path.Join("Application Files", Path.GetFileNameWithoutExtension(subpath) + "_" + stat.Version.Replace('.', '_')), result.Name);
            }
            else
            {
                // If version was empty or time expired 
                // just set raw sub path
                // to avoid duplicate like '/pro/en/pro/en/XLTools.vsto' as before.
                //                          ^^^^^^^
                manifestPath = subpath;
            }

            result = new VstoFileInfo(vstoRootPath, manifestPath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar));
        }
    }
}