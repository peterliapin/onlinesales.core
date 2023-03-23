// <copyright file="SendGridPlugin.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OnlineSales.Interfaces;
using OnlineSales.Plugin.SendGrid.Configuration;
using OnlineSales.Plugin.SendGrid.Tasks;

namespace OnlineSales.Plugin.SendGrid;

public class SendGridPlugin : IPlugin
{
    public static PluginConfig Configuration { get; private set; } = new PluginConfig();

    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        var pluginConfig = configuration.Get<PluginConfig>();

        if (pluginConfig != null)
        {
            Configuration = pluginConfig;
        }

        services.AddScoped<ITask, SyncSuppressionsTask>();
    }
}