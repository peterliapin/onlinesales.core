// <copyright file="AmazonSnsService.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using Amazon;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
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
        var accessKey = this.amazonSns.AccessKeyId;
        var secretKey = this.amazonSns.SecretAccessKey;
        var region = RegionEndpoint.GetBySystemName(this.amazonSns.DefaultRegion);

        var client = new AmazonSimpleNotificationServiceClient(accessKey, secretKey, region);
        var messageAttributes = new Dictionary<string, MessageAttributeValue>();

        messageAttributes["AWS.SNS.SMS.SMSType"] = new MessageAttributeValue
        {
            DataType = "String",
            StringValue = "Transactional",
        };

        messageAttributes["AWS.SNS.SMS.SenderID"] = new MessageAttributeValue
        {
            DataType = "String",
            StringValue = this.amazonSns.SenderId,
        };

        var request = new PublishRequest
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

    public string GetSender(string recipient)
    {
        return this.amazonSns.SenderId;
    }
}