// <copyright file="WaveServicePlugin.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OnlineSales.Interfaces;
using OnlineSales.Plugin.WaveService.Services;

namespace OnlineSales.Plugin.WaveService;
public class WaveServicePlugin : IPlugin
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<ResourceService>();
    }
}