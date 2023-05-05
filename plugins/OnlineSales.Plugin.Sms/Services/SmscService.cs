// <copyright file="SmscService.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Reflection;
using System.Web;
using Newtonsoft.Json;
using OnlineSales.Plugin.Sms.Configuration;
using OnlineSales.Plugin.Sms.Exceptions;
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
        var responseString = string.Empty;
        var args =
            $"login={HttpUtility.UrlEncode(this.smscConfig.Login)}" +
            $"&psw={HttpUtility.UrlEncode(this.smscConfig.Password)}" +
            $"&phones={HttpUtility.UrlEncode(recipient)}" +
            $"&mes={HttpUtility.UrlEncode(message)}" +
            $"&sender={HttpUtility.UrlEncode(this.smscConfig.SenderId)}" +
            "&fmt=3" + // Use JSON return format
            "&charset=utf-8";

        using (var httpClient = new HttpClient())
        {
            var response = await httpClient.GetAsync(this.smscConfig.ApiUrl + "?" + args);
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
        return this.smscConfig.SenderId;
    }
}