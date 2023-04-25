// <copyright file="ResourceService.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>
using System.Resources;

namespace OnlineSales.Plugin.WaveService.Services;
public class ResourceService
{
    private readonly ResourceManager resourceManger;

    public ResourceService()
    {
        resourceManger = new ResourceManager("OnlineSales.Plugin.WaveService.Resources.StoredFiles", typeof(ResourceService).Assembly);
    }

    public byte[]? GetFile(string fileName, string additional)
    {
        return (byte[]?)resourceManger.GetObject(fileName + additional);
    }

    public string? GetDescription(string fileName)
    {
        return resourceManger.GetString(fileName + "_txt");
    }
}
