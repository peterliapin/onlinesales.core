// <copyright file="IPlugin.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>
namespace OnlineSales.Interfaces
{
    public interface IPlugin : IDisposable
    {
        public string Name { get; }

        public string Description { get; }

        public string Version { get; }

        public string SettingsPath { get; }

        public Task OnInitialize();

        public void ConfigureServices(IServiceCollection services, IConfiguration configuration);

        public Task OnShutdown();
    }
}