// <copyright file="AmazonSnsService.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using Amazon;
using Amazon.Runtime;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using OnlineSales.Interfaces;
using OnlineSales.Plugin.Sms.Configuration;
using OnlineSales.Plugin.Sms.Exceptions;
using Serilog;

namespace OnlineSales.Plugin.Sms.Services;

public class AmazonSnsGatewayService : ISmsService
{
    private readonly AmazonSnsConfig amazonSns;

    public AmazonSnsGatewayService(AmazonSnsConfig amazonSns)
    {
        this.amazonSns = amazonSns;
    }

    public async Task SendAsync(string recipient, string message)
    {
        var accessKey = amazonSns.AccessKeyId;
        var secretKey = amazonSns.SecretAccessKey;
        RegionEndpoint region = RegionEndpoint.GetBySystemName(amazonSns.DefaultRegion);

        var client = new AmazonSimpleNotificationServiceClient(accessKey, secretKey, region);
        var messageAttributes = new Dictionary<string, MessageAttributeValue>();
        var smsType = new MessageAttributeValue
        {
            DataType = "String",
            StringValue = "Transactional",
        };

        messageAttributes.Add("AWS.SNS.SMS.SMSType", smsType);

        PublishRequest request = new PublishRequest
        {
            Message = message,
            PhoneNumber = recipient,
            MessageAttributes = messageAttributes,
        };

        var response = await client.PublishAsync(request);

        if (response.HttpStatusCode != System.Net.HttpStatusCode.OK)
        {
            throw new AwsSnsException("Failed to send Messages through SNS : Receiver : {" + recipient + "} St : { " + response.HttpStatusCode.ToString() + " }");
        }

        Log.Information("Sms message sent to {0} via AmazonSns gateway: {1} HttpStatus Code {2} Message Id {3}", recipient, message, response.HttpStatusCode.ToString(), response.MessageId.ToString());
    }
}

