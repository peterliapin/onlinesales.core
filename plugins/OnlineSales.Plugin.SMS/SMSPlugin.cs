// <copyright file="SMSPlugin.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace OnlineSales.Plugin.SMS;

public class SmsPlugin : IPlugin
{
    public void Configure(IServiceCollection services, IConfiguration configuration)
    {
        // nothing here
    }
}
