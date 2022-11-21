// <copyright file="AmazonSnsService.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using OnlineSales.Interfaces;
using OnlineSales.Plugin.SMS.Configuration;
using Serilog;

namespace OnlineSales.Plugin.SMS.Services;

public class AmazonSnsGatewayService : ISmsService
{
    private readonly AmazonSnsConfig amazonSns;

    public AmazonSnsGatewayService(AmazonSnsConfig amazonSns)
    {
        this.amazonSns = amazonSns;
    }

    public Task SendAsync(string recipient, string message)
    {
        Log.Information("Sms message sent to {0} via AmazonSns gateway: {1}", recipient, message);

        // TODO: Implement real SMS handling via AWS

        return Task.Delay(0);
    }
}

