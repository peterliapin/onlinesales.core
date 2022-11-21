// <copyright file="GetshoutoutService.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using OnlineSales.Plugin.Sms.Configuration;
using OnlineSales.Plugin.Sms.DTOs;
using Serilog;

namespace OnlineSales.Plugin.Sms.Services;

public class GetshoutoutService : ISmsService
{
    private static readonly JsonSerializerOptions SerializeOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
    };

    private readonly GetshoutoutConfig getshoutoutConfig;

    public GetshoutoutService(GetshoutoutConfig getshoutoutConfig)
    {
        this.getshoutoutConfig = getshoutoutConfig;
    }

    public async Task SendAsync(string recipient, string message)
    {       
        var client = new HttpClient();
        client.BaseAddress = new Uri(getshoutoutConfig.ApiUrl);
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        client.DefaultRequestHeaders.Add("Authorization", "Apikey " + getshoutoutConfig.ApiKey);

        var messageDto = new GetshoutoutMessageDto
        {
            Source = getshoutoutConfig.SenderId,
            Content = new Content
            {
                Sms = message,
            },
            Destinations = new List<string>
            {
                recipient.Substring(1, recipient.Length - 1), // removes "+" in front of the number as the Getshoutout does not want it
            },
        };

        var messageDtoJson = JsonSerializer.Serialize(messageDto, SerializeOptions);
        var content = new StringContent(messageDtoJson, new MediaTypeHeaderValue("application/json"));

        var result = await client.PostAsync("/coreservice/messages", content);

        result.EnsureSuccessStatusCode();

        Log.Information("Sms message sent to {0} via Getshoutout gateway: {1}", recipient, message);
    }
}

