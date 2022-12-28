// <copyright file="EmailWithLogService.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using Microsoft.EntityFrameworkCore;
using OnlineSales.Data;
using OnlineSales.DTOs;
using OnlineSales.Entities;
using OnlineSales.Interfaces;

namespace OnlineSales.Services
{
    public class EmailWithLogService : IEmailWithLogService
    {
        private readonly IEmailService emailService;
        private readonly ApiDbContext apiDbContext;

        public EmailWithLogService(IEmailService emailService, ApiDbContext apiDbContext)
        {
            this.emailService = emailService;
            this.apiDbContext = apiDbContext;
        }

        public async Task SendAsync(string subject, string fromEmail, string fromName, string[] recipients, string body, List<AttachmentDto>? attachments, int templateId = 0)
        {
            var emailStatus = false;
            var emails = string.Join(";", recipients);

            try
            {
                await emailService.SendAsync(subject, fromEmail, fromName, recipients, body, attachments);
                emailStatus = true;

                Log.Information($"Email with subject {subject} sent to {recipients} from {fromEmail}");
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error occurred when sending email with subject {subject} to {emails} from {fromEmail}");

                throw;
            }
            finally
            {
                await AddEmailLogEntry(subject, fromEmail, body, emails, emailStatus, templateId: templateId);
            }
        }

        public async Task SendToCustomerAsync(int customerId, string subject, string fromEmail, string fromName, string body, List<AttachmentDto>? attachments, int scheduleId = 0, int templateId = 0)
        {
            var emailStatus = false;
            var recipient = string.Empty;

            try
            {
                recipient = await GetCustomerEmailById(customerId);

                var recipientCollection = new[] { recipient };

                await emailService.SendAsync(subject, fromEmail, fromName, recipientCollection, body, attachments);
                emailStatus = true;

                Log.Information($"Email with subject {subject} sent to {recipient} from {fromEmail}");
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error occurred when sending email with subject {subject} to {recipient} from {fromEmail}");
                throw;
            }
            finally
            {
                await AddEmailLogEntry(subject, fromEmail, body, recipient, emailStatus, customerId, scheduleId, templateId);
            }
        }

        private async Task AddEmailLogEntry(string subject, string fromEmail, string body, string recipient, bool status, int customerId = 0, int scheduleId = 0, int templateId = 0)
        {
            try
            {
                var log = new EmailLog();

                if (customerId > 0)
                {
                    log.CustomerId = customerId;
                }

                if (scheduleId > 0)
                {
                    log.ScheduleId = scheduleId;
                }

                if (templateId > 0)
                {
                    log.TemplateId = templateId;
                }

                log.Subject = subject;
                log.FromEmail = fromEmail;
                log.Body = body;
                log.Recipient = recipient;
                log.Status = status ? EmailStatus.Sent : EmailStatus.NotSent;
                log.CreatedAt = DateTime.UtcNow;

                await apiDbContext.EmailLogs!.AddAsync(log);
                await apiDbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error occurred when adding a email log entry.");
            }
        }

        private async Task<string> GetCustomerEmailById(int customerId)
        {
            var customer = await apiDbContext.Customers!.FirstOrDefaultAsync(x => x.Id == customerId);

            return customer!.Email;
        }
    }
}