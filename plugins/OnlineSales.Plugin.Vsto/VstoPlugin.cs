// <copyright file="VstoPlugin.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OnlineSales.Data;
using OnlineSales.Plugin.Vsto.Configuration;
using OnlineSales.Plugin.Vsto.Data;

namespace OnlineSales.Plugin.Vsto;

public class VstoPlugin : IPlugin, IPluginApplication
{
    private IServiceCollection? services;

    public static PluginConfig Configuration { get; private set; } = new PluginConfig();

    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        var pluginConfig = configuration.Get<PluginConfig>();

        if (pluginConfig != null)
        {
            Configuration = pluginConfig;
        }

        this.services = services.AddScoped<PluginDbContextBase, VstoDbContext>();
    }

    public void ConfigureApplication(IApplicationBuilder application)
    {
        var httpContextHelper = application.ApplicationServices.GetRequiredService<IHttpContextHelper>();

        var assemblyPath = typeof(VstoDbContext).Assembly.Location;
        var pluginDirectory = Path.GetDirectoryName(assemblyPath);
        application.UseStaticFiles(new StaticFileOptions
        {
            RequestPath = Configuration.Vsto.RequestPath,
            FileProvider = new VstoFileProvider(Path.Combine(pluginDirectory!, Configuration.Vsto.LocalPath), httpContextHelper, services!),
            ServeUnknownFileTypes = true,
        });
    }
}