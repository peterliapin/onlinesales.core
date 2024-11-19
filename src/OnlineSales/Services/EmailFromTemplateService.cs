// <copyright file="EmailFromTemplateService.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OnlineSales.Configuration;
using OnlineSales.Data;
using OnlineSales.DTOs;
using OnlineSales.Entities;
using OnlineSales.Helpers;
using OnlineSales.Infrastructure;
using OnlineSales.Interfaces;

namespace OnlineSales.Services
{
    public class EmailFromTemplateService : IEmailFromTemplateService
    {
        private readonly IEmailWithLogService emailWithLogService;
        private readonly PgDbContext pgDbContext;
        private readonly IOptions<ApiSettingsConfig> apiSettingsConfig;

        public EmailFromTemplateService(IEmailWithLogService emailWithLogService, PgDbContext pgDbContext, IOptions<ApiSettingsConfig> apiSettingsConfig)
        {
            this.emailWithLogService = emailWithLogService;
            this.pgDbContext = pgDbContext;
            this.apiSettingsConfig = apiSettingsConfig;
        }

        public async Task SendAsync(string templateName, string language, string[] recipients, Dictionary<string, string>? templateArguments, List<AttachmentDto>? attachments)
        {
            var template = await GetEmailTemplate(templateName, language);

            var body = EvaluateTemplate(template.BodyTemplate, templateArguments);
            var subject = EvaluateTemplate(template.Subject, templateArguments);

            await emailWithLogService.SendAsync(subject, template.FromEmail, template.FromName, recipients, body, attachments, template.Id);
        }

        public async Task SendToContactAsync(int contactId, string templateName, Dictionary<string, string>? templateArguments, List<AttachmentDto>? attachments, int scheduleId = 0)
        {
            var template = await GetEmailTemplate(templateName, contactId);

            var body = EvaluateTemplate(template.BodyTemplate, templateArguments);
            var subject = EvaluateTemplate(template.Subject, templateArguments);

            await emailWithLogService.SendToContactAsync(contactId, subject, template.FromEmail, template.FromName, body, attachments, scheduleId, template.Id);
        }

        private static string EvaluateTemplate(string template, Dictionary<string, string>? templateArguments)
        {
            if (templateArguments is null)
            {
                return template;
            }

            var result = TokenHelper.ReplaceTokensFromVariables(templateArguments!.ConvertKeys("<%", "%>"), template);
            result = TokenHelper.ReplaceTokensFromVariables(templateArguments!.ConvertKeys("${", "}"), result);
            return result;
        }

        private async Task<EmailTemplate> GetEmailTemplate(string name, string language)
        {
            var template = await GetEmailTemplateByLanguage(name, language);

            return template!;
        }

        private async Task<EmailTemplate> GetEmailTemplate(string name, int contactId)
        {
            var contact = await pgDbContext.Contacts!.FirstOrDefaultAsync(c => c.Id == contactId);

            var language = contact!.Language;

            var template = await GetEmailTemplateByLanguage(name, language);

            return template!;
        }

        private async Task<EmailTemplate?> GetEmailTemplateByLanguage(string name, string? language)
        {
            string defaultLang = apiSettingsConfig.Value.DefaultLanguage!;

            // set default if not set
            language ??= defaultLang;

            if (language.Length == 2)
            {
                var twoLetterBasedLangMatch = await pgDbContext.EmailTemplates!
                    .Where(x => x.Name == name && x.Language.StartsWith(language))
                    .OrderBy(x => x.Language)
                    .FirstOrDefaultAsync();

                if (twoLetterBasedLangMatch != null)
                {
                    return twoLetterBasedLangMatch;
                }
            }

            // try to find template by provided language
            var template = await pgDbContext.EmailTemplates!.FirstOrDefaultAsync(x => x.Name == name && x.Language == language);

            // if template not found, try find with default language
            template ??= await pgDbContext.EmailTemplates!.FirstOrDefaultAsync(x => x.Name == name && x.Language == defaultLang);

            return template;
        }
    }
}