// <copyright file="VstoPlugin.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace OnlineSales.Plugin.Vsto;

public class VstoPlugin : IPlugin
{
    public VstoPlugin()
    {
    }

    public void Configure(IServiceCollection services, IConfiguration configuration)
    {
        // nothing   
    }
}

