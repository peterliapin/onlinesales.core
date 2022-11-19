// <copyright file="AzureADPlugin.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace OnlineSales.Plugin.AzureAD;

public class AzureADPlugin : IPlugin
{
    public void Configure(IServiceCollection services, IConfiguration configuration)
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}