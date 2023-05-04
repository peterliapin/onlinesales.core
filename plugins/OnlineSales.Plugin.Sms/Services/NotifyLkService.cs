// <copyright file="NotifyLkService.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Net.Http.Headers;
using Microsoft.AspNetCore.WebUtilities;
using OnlineSales.Plugin.Sms.Configuration;
using OnlineSales.Plugin.Sms.Exceptions;
using Serilog;

namespace OnlineSales.Plugin.Sms.Services
{
    public class NotifyLkService : ISmsService
    {
        private readonly NotifyLkConfig notifyLkConfig;

        public NotifyLkService(NotifyLkConfig notifyLkConfig)
        {
            this.notifyLkConfig = notifyLkConfig;
        }

        public async Task SendAsync(string recipient, string message)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var queryParams = new Dictionary<string, string>
            {
                ["user_id"] = this.notifyLkConfig.UserId,
                ["api_key"] = this.notifyLkConfig.ApiKey,
                ["sender_id"] = this.notifyLkConfig.SenderId,
                ["to"] = recipient.Substring(1, recipient.Length - 1),
                ["message"] = message,
            };

            var response = await client.GetAsync(QueryHelpers.AddQueryString(this.notifyLkConfig.ApiUrl, queryParams!));

            if (response.IsSuccessStatusCode)
            {
                Log.Information("Sms message sent to {0} via NotifyLK gateway: {1}", recipient, message);
            }
            else
            {
                var responseContent = await response.Content.ReadAsStringAsync();

                throw new NotifyLkException(responseContent);
            }
        }

        public string GetSender(string recipient)
        {
            return this.notifyLkConfig.SenderId;
        }
    }
}