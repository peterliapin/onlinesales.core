// <copyright file="SmscService.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using OnlineSales.Infrastructure;
using OnlineSales.Plugin.Sms.Configuration;
using Serilog;

namespace OnlineSales.Plugin.Sms.Services;

public class SmscService : ISmsService
{
    private readonly SmscConfig smscConfig;

    public SmscService(SmscConfig smscConfig)
    {
        this.smscConfig = smscConfig;
    }

    public Task SendAsync(string recipient, string message)
    {
        Log.Information("Sms message sent to {0} via Smsc gateway: {1}", recipient, message);

        // TODO: Implement real SMS handling via smsc.ru

        return Task.Delay(0);
    }
}

