// <copyright file="TwilioService.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>
using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using OnlineSales.Plugin.Sms.Configuration;
using Serilog;
using Twilio;
using Twilio.Exceptions;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace OnlineSales.Plugin.Sms.Services
{
    public class TwilioService : ISmsService
    {
        private readonly TwilioConfig twilioConfig;

        public TwilioService(TwilioConfig twilioCfg)
        {
            twilioConfig = twilioCfg;
            try
            {
                TwilioClient.Init(twilioCfg.AccountSid, twilioCfg.AuthToken);
            }
            catch (ApiException e)
            {
                Log.Error("Failed to init twillio client {0}", e.Message);
            }
        }

        public string GetSender(string recipient)
        {
            return twilioConfig.SenderId;
        }

        public async Task SendAsync(string recipient, string message)
        {
            var options = new CreateMessageOptions(new PhoneNumber(recipient))
            {
                From = new PhoneNumber(twilioConfig.SenderId),
                Body = message,
            };

            await MessageResource.CreateAsync(options);
            Log.Information("Sms message sent to {0} via Twilio gateway: {1}", recipient, message);
        }
    }
}