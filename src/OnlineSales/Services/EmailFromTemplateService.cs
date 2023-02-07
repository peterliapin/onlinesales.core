// <copyright file="EmailFromTemplateService.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using Microsoft.EntityFrameworkCore;
using OnlineSales.Data;
using OnlineSales.DTOs;
using OnlineSales.Entities;
using OnlineSales.Infrastructure;
using OnlineSales.Interfaces;

namespace OnlineSales.Services
{
    public class EmailFromTemplateService : IEmailFromTemplateService
    {
        private const string DefaultLanguage = "en";

        private readonly IEmailWithLogService emailWithLogService;
        private readonly PgDbContext apiDbContext;

        public EmailFromTemplateService(IEmailWithLogService emailWithLogService, PgDbContext apiDbContext)
        {
            this.emailWithLogService = emailWithLogService;
            this.apiDbContext = apiDbContext;
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
            var template = await apiDbContext.EmailTemplates!.FirstOrDefaultAsync(x => x.Name == name && x.Language == GetSupportedLanguage(language));

            return template!;
        }

        private async Task<EmailTemplate> GetEmailTemplate(string name, int contactId)
        {
            var contactLanguage = apiDbContext.Contacts!.FirstOrDefault(c => c.Id == contactId) !.Language;

            var template = await apiDbContext.EmailTemplates!.FirstOrDefaultAsync(x => x.Name == name && x.Language == GetSupportedLanguage(contactLanguage));

            return template!;
        }

        private string GetSupportedLanguage(string? language)
        {
            var supportedLanguages = apiDbContext.EmailTemplates!.Select(e => e.Language).Distinct().ToArray();

            if (supportedLanguages.Contains(language, StringComparer.OrdinalIgnoreCase))
            {
                return language!.ToLower();
            }

            return DefaultLanguage;
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