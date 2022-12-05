// <copyright file="VstoPlugin.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OnlineSales.Infrastructure;
using OnlineSales.Plugin.Vsto.Configuration;

namespace OnlineSales.Plugin.Vsto;

public class VstoPlugin : IPlugin, IPluginApplication
{
    public static PluginConfig Configuration { get; private set; } = new PluginConfig();

    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        var pluginConfig = configuration.Get<PluginConfig>();

        if (pluginConfig != null)
        {
            Configuration = pluginConfig;
        }
    }

    public void ConfigureApplication(IApplicationBuilder application)
    {
        /* var httpContextHelper = application.ApplicationServices.GetRequiredService<HttpContextHelper>();

        application.UseStaticFiles(new StaticFileOptions
        {
            RequestPath = Configuration.Vsto.RequestPath,
            FileProvider = new VstoFileProvider(httpContextHelper),
            ServeUnknownFileTypes = true,
        });*/
    }
}

