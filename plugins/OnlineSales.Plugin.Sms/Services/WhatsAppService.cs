// <copyright file="WhatsAppService.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using OnlineSales.Plugin.Sms.Configuration;
using OnlineSales.Plugin.Sms.DTOs;
using OnlineSales.Plugin.Sms.Interfaces;

namespace OnlineSales.Plugin.Sms.Services
{
    public class WhatsAppService : IWhatsAppService, ISmsService, IDisposable
    {
        private readonly bool enableLinkPreviewByDefault;
        private readonly HttpClient? client;

        public WhatsAppService(WhatsAppConfig config)
        {
            if (config.AuthToken == "$SMSGATEWAYS__WHATSAPP__AUTHTOKEN" || config.BusinessPhoneId == "$SMSGATEWAYS__WHATSAPP__BUSINESSPHONEID")
            {
                client = null;
            }
            else
            {
                var relative = $"{config.ApiVersion}/{config.BusinessPhoneId}";
                client = new HttpClient()
                {
                    BaseAddress = new Uri(config.ApiUrl, relative),
                };
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", config.AuthToken);
            }

            enableLinkPreviewByDefault = config.EnableLinkPreviewByDefault;
        }

        public WhatsAppService()
            : this(SmsPlugin.Configuration.SmsGateways.WhatsApp)
        {
        }

        public string GetSender(string recipient)
        {
            return "WhatsApp";
        }

        public async Task SendAsync(string recipient, string message)
        {
            await SendTextMessage(recipient, message, enableLinkPreviewByDefault);
        }

        public async Task<bool> SendAuthTemplateMessage(string phone, string language, string otpCode)
        {
            if (client == null)
            {
                return false;
            }

            var templateConfig = SmsPlugin.Configuration.OtpGateways.WhatsApp.GetByLanguage(language);

            var dto = new WhatsAppSendAuthTemplateMessageDto()
            {
                to = phone,
            };
            dto.template.name = templateConfig.TemplateName;
            dto.template.language.code = templateConfig.LanguageCode;
            dto.template.components[0].parameters![0].text =
                dto.template.components[1].parameters![0].text = otpCode;

            var jsonDto = JsonSerializer.Serialize(dto);
            var content = new StringContent(jsonDto, Encoding.UTF8, new MediaTypeHeaderValue("application/json"));

            var response = await client.PostAsync("messages", content);

            return response.IsSuccessStatusCode;
        }

        public Task<bool> SendTextMessage(string phone, string text, bool enableLinkPreview)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            client?.Dispose();
        }
    }
}
