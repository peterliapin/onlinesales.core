// <copyright file="TestPlugin.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OnlineSales.Data;
using OnlineSales.Interfaces;
using OnlineSales.Plugin.TestPlugin.Data;
using OnlineSales.Tests.Interfaces;

namespace OnlineSales.Plugin.TestPlugin
{
    public class TestPlugin : IPlugin
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddAutoMapper(typeof(TestPlugin));

            services.AddScoped<ITestMigrationService, TestPluginDbContext>();
            services.AddScoped<PluginDbContextBase, TestPluginDbContext>();
            services.AddScoped<TestPluginDbContext, TestPluginDbContext>();
        }
    }
}