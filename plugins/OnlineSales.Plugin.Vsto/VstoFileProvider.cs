// <copyright file="VstoFileProvider.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

namespace OnlineSales.Plugin.Vsto;

public enum VstoFileType
{
    Exe,
    Vsto,
    None,
}

public class VstoFileProvider : IFileProvider
{
    private readonly string vstoRootPath;
    private readonly IHttpContextHelper httpContextHelper;

    public VstoFileProvider(IHttpContextHelper httpContextHelper)
    {
        vstoRootPath = System.IO.Directory.GetCurrentDirectory();
        this.httpContextHelper = httpContextHelper;
    }

    public IDirectoryContents GetDirectoryContents(string subpath)
    {
        throw new NotImplementedException();
    }

    public IFileInfo GetFileInfo(string subpath)
    {
        var result = new VstoFileInfo(vstoRootPath, subpath);

        try
        {
            if (ParseContext(out var fileType, out var ipAdress, out var version, subpath))
            {
                if (fileType == VstoFileType.Exe)
                {
                    if (!string.IsNullOrEmpty(version) && !string.IsNullOrEmpty(ipAdress))
                    {
                       // db.WriteVstoExeDownload(ipAdress, version);
                    }
                }
                else if (fileType == VstoFileType.Vsto)
                {
                    // var stat = db.GetUserVersion(ipAdress);
                    //  if (stat != null && stat.TTL > DateTime.Now && !string.IsNullOrEmpty(stat.Version))
                    //  {
                    //      var versionPath = BuildPath(stat, subpath);
                    //      var versionFile = new VSTOFileInfo(_path, versionPath);
                    //      if (versionFile.Exists)
                    //      {
                    //          db.MarkAsInstalled(stat);
                    //          return versionFile;
                    //      }
                    //  }
                }
            }
        }
        catch (Exception)
        {
            // nothing
        }

        return result.Exists ? result as IFileInfo : new NotFoundFileInfo(subpath);
    }

    public IChangeToken Watch(string filter)
    {
        throw new NotImplementedException();
    }

    private bool ParseContext(out VstoFileType fileType, out string ipAdress, out string version, string subpath)
    {
        // var fileName = System.IO.Path.GetFileName(subpath).ToLower();
        var result = false;

        if (!Enum.TryParse(System.IO.Path.GetExtension(subpath).Replace(".", string.Empty), true, out fileType))
        {
            fileType = VstoFileType.None;
        }

        ipAdress = httpContextHelper.IpAddress!;
        /* var request = httpContextHelper.Request!;

        version = request!.Query["version"];

        if (string.IsNullOrEmpty(version))
        {
            version = request?.Query["ver"];
        }

        if (string.IsNullOrEmpty(version))
        {
            version = request.Query["v"];
        } */

        version = string.Empty;

        return result;
    }
}

