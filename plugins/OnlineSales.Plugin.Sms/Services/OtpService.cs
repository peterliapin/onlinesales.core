// <copyright file="OtpService.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

namespace OnlineSales.Plugin.Sms.Services
{
    public class OtpService : IOtpService
    {
        private readonly ITelegramService telegramService;
        private readonly ISmsService smsService;

        public OtpService(ITelegramService telegramService, ISmsService smsService)
        {
            this.telegramService = telegramService;
            this.smsService = smsService;
        }

        public async Task SendOtpAsync(string recepient, string code)
        {
            var checkResult = await telegramService.CanDeliverAsync(recepient);
            if (checkResult == null)
            {
                await smsService.SendAsync(recepient, code);
            }
            else
            {
                await telegramService.SendAsync(recepient, checkResult!.request_id, code);
            }
        }
    }
}
