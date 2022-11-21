// <copyright file="SmsPlugin.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OnlineSales.Plugin.SMS.Configuration;
using OnlineSales.Plugin.SMS.Services;

namespace OnlineSales.Plugin.SMS;

public class SmsPlugin : IPlugin
{
    public static PluginSettings Settings { get; private set; } = new PluginSettings();

    public void Configure(IServiceCollection services, IConfiguration configuration)
    {
        var pluginSettings = configuration.Get<PluginSettings>();

        if (pluginSettings != null)
        {
            Settings = pluginSettings;
        }

        services.AddSingleton<ISmsService, SmsService>();
    }
}
