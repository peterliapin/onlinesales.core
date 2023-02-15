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
        private readonly Twilio.Base.ResourceSet<IncomingPhoneNumberResource> availablePhoneNumbers;

        public TwilioService(TwilioConfig twilioCfg)
        {
            this.twilioConfig = twilioCfg;
            TwilioClient.Init(twilioCfg.AccountSid, twilioCfg.AuthToken);
            availablePhoneNumbers = IncomingPhoneNumberResource.Read();
        }

        public string GetSender(string recipient)
        {
            return TwilioClient.GetRestClient().AccountSid;
        }

        public async Task SendAsync(string recipient, string message)
        {
            if (!availablePhoneNumbers.Any())
            {
                throw new TwilioException("No available phonenumbers to send message from. Check account dashboard.");
            }

            var options = new CreateMessageOptions(new PhoneNumber(recipient))
            {
                From = availablePhoneNumbers.First().PhoneNumber,
                Body = message,
            };

            await MessageResource.CreateAsync(options);
            Log.Information("Sms message sent to {0} via Twilio gateway: {1}", recipient, message);
        }
    }
}
