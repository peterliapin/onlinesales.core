// <copyright file="IPlugin.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

namespace OnlineSales.Interfaces
{
    public interface IPlugin
    {
        public void Configure(IServiceCollection services, IConfiguration configuration);
    }
}