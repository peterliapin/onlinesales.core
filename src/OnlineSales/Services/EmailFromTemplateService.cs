// <copyright file="EmailFromTemplateService.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using Microsoft.EntityFrameworkCore;
using OnlineSales.Configuration;
using OnlineSales.Data;
using OnlineSales.DTOs;
using OnlineSales.Entities;
using OnlineSales.Infrastructure;
using OnlineSales.Interfaces;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace OnlineSales.Services
{
    public class EmailFromTemplateService : IEmailFromTemplateService
    {
        private readonly IEmailWithLogService emailWithLogService;
        private readonly PgDbContext pgDbContext;
        private readonly DefaultLanguage config;

        public EmailFromTemplateService(IEmailWithLogService emailWithLogService, PgDbContext pgDbContext, IConfiguration configuration)
        {
            this.emailWithLogService = emailWithLogService;
            this.pgDbContext = pgDbContext;

            var settings = configuration.GetSection("DefaultLanguage").Get<DefaultLanguage>();

            if (settings != null)
            {
                config = settings;
            }
            else
            {
                throw new MissingConfigurationException($"The specified configuration section for the type {typeof(DefaultLanguage).FullName} could not be found in the settings file.");
            }
        }

        public async Task SendAsync(string templateName, string language, string[] recipients, Dictionary<string, string>? templateArguments, List<AttachmentDto>? attachments)
        {
            var template = await GetEmailTemplate(templateName, language);

            var updatedBodyTemplate = GetUpdatedBodyTemplate(template.BodyTemplate, templateArguments);

            await emailWithLogService.SendAsync(template.Subject, template.FromEmail, template.FromName, recipients, updatedBodyTemplate, attachments, template.Id);
        }

        public async Task SendToContactAsync(int contactId, string templateName, Dictionary<string, string>? templateArguments, List<AttachmentDto>? attachments, int scheduleId = 0)
        {
            var template = await GetEmailTemplate(templateName, contactId);

            var updatedBodyTemplate = GetUpdatedBodyTemplate(template.BodyTemplate, templateArguments);

            await emailWithLogService.SendToContactAsync(contactId, template.Subject, template.FromEmail, template.FromName, updatedBodyTemplate, attachments, scheduleId, template.Id);
        }

        private async Task<EmailTemplate> GetEmailTemplate(string name, string language)
        {
            var template = await GetEmailTemplateByLanguage(name, language);

            return template!;
        }

        private async Task<EmailTemplate> GetEmailTemplate(string name, int contactId)
        {
            var contactLanguage = pgDbContext.Contacts!.FirstOrDefault(c => c.Id == contactId)!.Language;

            var template = await GetEmailTemplateByLanguage(name, contactLanguage);

            return template!;
        }

        private async Task<EmailTemplate?> GetEmailTemplateByLanguage(string name, string? language)
        {
            string defLang = config.Language!;

            // set default if not set
            language ??= defLang;

            // try find template by provided language
            // with 2 and 5 language codes
            var template = await pgDbContext.EmailTemplates!.FirstOrDefaultAsync(x => x.Name == name && (x.Language.Length == 2 ? x.Language == language.Substring(0, 2) : x.Language == language));

            // if template not found, try find with default language
            if (template == null)
            {
                template = await pgDbContext.EmailTemplates!.FirstOrDefaultAsync(x => x.Name == name && (x.Language.Length == 2 ? x.Language == defLang.Substring(0, 2) : x.Language == defLang));
            }

            return template;
        }

        private string GetUpdatedBodyTemplate(string bodyTemplate, Dictionary<string, string>? templateArguments)
        {
            if (templateArguments is null)
            {
                return bodyTemplate;
            }

            return TokenHelper.ReplaceTokensFromVariables(templateArguments!.ConvertKeys("<%", "%>"), bodyTemplate);
        }
    }
}