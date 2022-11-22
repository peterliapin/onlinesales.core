// <copyright file="AmazonSnsService.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using Amazon;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using OnlineSales.Interfaces;
using OnlineSales.Plugin.Sms.Configuration;
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
        // string awsKeyId = "AKIAQOCM6Q5CFR5VOUNU";
        // string awsKeySecret = "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";

        /* var awsCredentials = new BasicAWSCredentials(awsKeyId, awsKeySecret);
         AmazonSimpleNotificationServiceClient snsClient = new AmazonSimpleNotificationServiceClient(awsCredentials);
         PublishRequest publishRequest = new ();

         publishRequest.Message = "This is a test message from API";
         publishRequest.PhoneNumber = "+94778926711";

         publishRequest.MessageAttributes.Add("AWS.SNS.SMS.SMSType", new MessageAttributeValue { StringValue = "Transactional", DataType = "string" });

         PublishResponse response = await snsClient.PublishAsync(publishRequest);

         Log.Information("Sms message sent to {0} via AmazonSns gateway: {1} Metea Data {2}", recipient, message, response.ResponseMetadata.ToString());*/

        // TODO: Implement real SMS handling via AWS

        var accessKey = "AKIAQOCM6Q5CFR5VOUNU";
        var secretKey = "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";
        var client = new AmazonSimpleNotificationServiceClient(accessKey, secretKey, RegionEndpoint.USEast1);
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
            PhoneNumber = "+94778926711",
            MessageAttributes = messageAttributes,
        };

        await client.PublishAsync(request);

         // return Task.Delay(0);
    }
}

