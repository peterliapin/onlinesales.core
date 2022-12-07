// <copyright file="EmailPlugin.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OnlineSales.Plugin.Email.Configuration;
using OnlineSales.Plugin.Email.Services;

namespace OnlineSales.Plugin.Email
{
    public class EmailPlugin : IPlugin
    {
        public static PluginSettings Settings { get; private set; } = new PluginSettings();

        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            var pluginSettings = configuration.Get<PluginSettings>();

            if (pluginSettings != null)
            {
                Settings = pluginSettings;
            }

            services.AddSingleton<IEmailService, EmailService>();
        }
    }
}