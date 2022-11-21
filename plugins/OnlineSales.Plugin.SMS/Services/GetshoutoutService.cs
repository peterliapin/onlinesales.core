// <copyright file="GetshoutoutService.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using OnlineSales.Interfaces;
using OnlineSales.Plugin.SMS.Configuration;
using Serilog;

namespace OnlineSales.Plugin.SMS.Services;

public class GetshoutoutService : ISmsService
{
    private readonly GetshoutoutConfig getshoutout;

    public GetshoutoutService(GetshoutoutConfig getshoutout)
    {
        this.getshoutout = getshoutout;
    }

    public Task SendAsync(string recipient, string message)
    {
        Log.Information("Sms message sent to {0} via Getshoutout gateway: {1}", recipient, message);

        // TODO: Implement real SMS handling via getshoutout.com

        return Task.Delay(0);
    }
}

