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

    public static string VstoLocalPath { get; private set; } = string.Empty;

    public static PluginConfig Configuration { get; private set; } = new PluginConfig();

    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        var pluginConfig = configuration.Get<PluginConfig>();

        if (pluginConfig != null)
        {
            Configuration = pluginConfig;
        }

        var assemblyPath = typeof(VstoDbContext).Assembly.Location;
        var pluginDirectory = Path.GetDirectoryName(assemblyPath);

        VstoLocalPath = Path.Combine(pluginDirectory!, Configuration.Vsto.LocalPath);

        services.AddScoped<PluginDbContextBase, VstoDbContext>();
        services.AddSingleton<IVariablesProvider, VstoVariablesProvider>();

        this.services = services;
    }

    public void ConfigureApplication(IApplicationBuilder application)
    {
        var httpContextHelper = application.ApplicationServices.GetRequiredService<IHttpContextHelper>();

        application.UseStaticFiles(new StaticFileOptions
        {
            RequestPath = Configuration.Vsto.RequestPath,
            FileProvider = new VstoFileProvider(VstoLocalPath, httpContextHelper, services!),
            ServeUnknownFileTypes = true,
        });
    }
}