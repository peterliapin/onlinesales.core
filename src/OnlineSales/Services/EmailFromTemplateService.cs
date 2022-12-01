// <copyright file="EmailFromTemplateService.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using Microsoft.EntityFrameworkCore;
using OnlineSales.Data;
using OnlineSales.Entities;
using OnlineSales.Interfaces;

namespace OnlineSales.Services
{
    public class EmailFromTemplateService : IEmailFromTemplateService
    {
        private readonly IEmailWithLogService emailWithLogService;
        private readonly ApiDbContext apiDbContext;

        public EmailFromTemplateService(IEmailWithLogService emailWithLogService, ApiDbContext apiDbContext)
        {
            this.emailWithLogService = emailWithLogService;
            this.apiDbContext = apiDbContext;
        }

        public async Task SendAsync(string templateName, string recipient, Dictionary<string, string> templateArguments)
        {
            var template = await GetTemplateByName(templateName);

            var updatedBodyTemplate = GetUpdatedBodyTemplate(template.BodyTemplate, templateArguments);

            await emailWithLogService.SendAsync(template.Subject, template.FromEmail, template.FromName, recipient, updatedBodyTemplate, null, template.Id);
        }

        public async Task SendToCustomerAsync(int customerId, string templateName, Dictionary<string, string> templateArguments, int scheduleId = 0)
        {
            var template = await GetTemplateByName(templateName);

            var updatedBodyTemplate = GetUpdatedBodyTemplate(template.BodyTemplate, templateArguments);

            await emailWithLogService.SendToCustomerAsync(customerId, template.Subject, template.FromEmail, template.FromName, updatedBodyTemplate, null, scheduleId, template.Id);
        }

        private async Task<EmailTemplate> GetTemplateByName(string name)
        {
            var template = await apiDbContext.EmailTemplates!.FirstOrDefaultAsync(x => x.Name == name);

            return template!;
        }

        private string GetUpdatedBodyTemplate(string bodyTemplate, Dictionary<string, string> templateArguments)
        {
            foreach (var arg in templateArguments)
            {
                bodyTemplate = bodyTemplate.Replace(arg.Key, arg.Value);
            }

            return bodyTemplate;
        }
    }
}
