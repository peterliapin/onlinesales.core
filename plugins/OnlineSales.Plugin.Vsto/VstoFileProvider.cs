// <copyright file="VstoFileProvider.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

namespace OnlineSales.Plugin.Vsto;

public class VstoFileProvider : IFileProvider
{
    public VstoFileProvider()
    {
    }

    public IDirectoryContents GetDirectoryContents(string subpath)
    {
        throw new NotImplementedException();
    }

    public IFileInfo GetFileInfo(string subpath)
    {
        throw new NotImplementedException();
    }

    public IChangeToken Watch(string filter)
    {
        throw new NotImplementedException();
    }
}

