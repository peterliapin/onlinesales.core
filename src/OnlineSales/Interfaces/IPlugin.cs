// <copyright file="IPlugin.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

namespace OnlineSales.Interfaces
{
    public interface IPlugin
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration);
    }

    public interface IPluginApplication
    {
        public void ConfigureApplication(IApplicationBuilder application);
    }
}