// <copyright file="SmscService.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Web;
using Newtonsoft.Json;
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

    public async Task SendAsync(string recipient, string message)
    {
        Log.Information("Sms message sent to {0} via Smsc gateway: {1}", recipient, message);

        await SmscSend(recipient, message);
    }

    private async Task<string> SmscSend(string phone, string message, string sender = "")
    {
        var responseString = string.Empty;
        var args =
            $"login={HttpUtility.UrlEncode(smscConfig.Login)}" +
            $"psw={HttpUtility.UrlEncode(smscConfig.Password)}" +
            $"{(sender == string.Empty ? string.Empty : HttpUtility.UrlEncode(sender))}" +
            $"&phones={HttpUtility.UrlEncode(phone)}" +
            $"&mes={HttpUtility.UrlEncode(message)}" +
            "fmt=3" + // Use JSON return format
            "charset=utf-8";

        using (var httpClient = new HttpClient())
        {
            var response = await httpClient.GetAsync(smscConfig.ApiUrl + "?" + args);
            responseString = await response.Content.ReadAsStringAsync();
        }

        dynamic jsonResponseObject = JsonConvert.DeserializeObject(responseString);
        if (jsonResponseObject["error"] != null)
        {
            Log.Error("Failed to send message to {0} ( {1} )", phone, jsonResponseObject["error"]);
        }

        return responseString;
    }
}

