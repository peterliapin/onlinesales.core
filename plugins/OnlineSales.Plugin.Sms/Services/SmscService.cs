// <copyright file="SmscService.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Text;
using Newtonsoft.Json;
using OnlineSales.Plugin.Sms.Configuration;
using OnlineSales.Plugin.Sms.Exceptions;

namespace OnlineSales.Plugin.Sms.Services;

public class SmscService : ISmsService
{
    private readonly SmscConfig smscConfig;

    public SmscService(SmscConfig smscConfig)
    {
        this.smscConfig = smscConfig;
    }

    public async Task SendAsync(string recipient, string message)
    {
        var responseString = string.Empty;
        var data = JsonConvert.SerializeObject(new
        {
            login = smscConfig.Login,
            psw = smscConfig.Password,
            phones = recipient,
            mes = message,
            sender = smscConfig.SenderId,
            fmt = 3,
            charset = "utf-8",
        });

        using (var httpClient = new HttpClient())
        {
            var response = await httpClient.PostAsync(smscConfig.ApiUrl, new StringContent(data, Encoding.UTF8));
            responseString = await response.Content.ReadAsStringAsync();
        }

        dynamic? jsonResponseObject = JsonConvert.DeserializeObject(responseString);
        if (jsonResponseObject != null && jsonResponseObject!["error"] != null)
        {
            throw new SmscException($"Failed to send message to {recipient} ( {jsonResponseObject!["error"]} )");
        }
    }

    public string GetSender(string recipient)
    {
        return smscConfig.SenderId;
    }
}