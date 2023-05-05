// <copyright file="VstoFileInfo.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using Microsoft.Extensions.FileProviders;

namespace OnlineSales.Plugin.Vsto;

public class VstoFileInfo : IFileInfo
{
    private readonly string filePath;
    private readonly string basePath;

    public VstoFileInfo(string basePath, string filePath)
    {
        this.basePath = basePath;
        this.filePath = filePath;
    }

    public string FullPath => Path.Join(this.basePath, this.filePath);

    public bool Exists => File.Exists(this.FullPath);

    public bool IsDirectory => false;

    public DateTimeOffset LastModified
    {
        get
        {
            return File.GetLastWriteTime(this.FullPath);
        }
    }

    public long Length
    {
        get
        {
            return File.ReadAllBytes(this.FullPath).Length;
        }
    }

    public string Name => Path.GetFileName(this.FullPath);

    public string PhysicalPath => string.Empty;

    public Stream CreateReadStream()
    {
        return File.OpenRead(this.FullPath);
    }
}