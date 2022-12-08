// <copyright file="CustomerScheduledEmail.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using Microsoft.EntityFrameworkCore;
using OnlineSales.Configuration;
using OnlineSales.Data;
using OnlineSales.Entities;
using OnlineSales.Interfaces;

namespace OnlineSales.Tasks;

public class CustomerScheduledEmail : ITask
{
    private readonly ApiDbContext dbContext;
    private readonly IEmailFromTemplateService emailFromTemplateService;
    private readonly TaskConfig? taskConfig = new TaskConfig();

    public CustomerScheduledEmail(ApiDbContext dbContext, IEmailFromTemplateService emailFromTemplateService, IConfiguration configuration)
    {
        this.dbContext = dbContext;
        this.emailFromTemplateService = emailFromTemplateService;

        var config = configuration.GetSection("Tasks:CustomerScheduledEmail") !.Get<TaskConfig>();
        if (config is not null)
        {
            taskConfig = config;
        }
    }

    public string Name => "CustomerScheduledEmailTask";

    public string CronSchedule => taskConfig!.CronSchedule;

    public int RetryCount => taskConfig!.RetryCount;

    public int RetryInterval => taskConfig!.RetryInterval;

    public async Task<bool> Execute(TaskExecutionLog currentJob)
    {
        // Load all the customers in CustomerEmailSchedule table by pending status
        var schedules = dbContext.CustomerEmailSchedules!.Where(s => s.Status == ScheduleStatus.Pending);

        foreach (var schedule in schedules)
        {
            EmailTemplate? nextEmailTemplateToSend;
            int retryDelay = 0;

            var lastEmailLog = dbContext.EmailLogs!.Where(
                    e => e.ScheduleId == schedule.ScheduleId &&
                    e.CustomerId == schedule.CustomerId).OrderByDescending(x => x.CreatedAt).FirstOrDefault();

            var lastEmailTempalte = dbContext.EmailTemplates!.Where(
                    e => e.Id == lastEmailLog!.TemplateId).FirstOrDefault();

            // Retry logic if the last email is not sent
            if (lastEmailLog!.Status is EmailStatus.NotSent)
            {
                var emailNotSentCount = dbContext.EmailLogs!.Where(
                        e => e.ScheduleId == schedule.ScheduleId
                        && e.CustomerId == schedule.CustomerId
                        && e.TemplateId == lastEmailLog.TemplateId
                        && e.Status == EmailStatus.NotSent).Count();

                // If all retry attempts completed, get the next email template to send.
                if (emailNotSentCount > lastEmailTempalte!.RetryCount)
                {
                    nextEmailTemplateToSend = GetNextEmailTemplateToSend(lastEmailTempalte);
                }
                else
                {
                    // Retry attempt available. sending the same email template.
                    nextEmailTemplateToSend = dbContext.EmailTemplates!.Where(t => t.Id == lastEmailTempalte.Id).FirstOrDefault();
                    retryDelay = lastEmailTempalte!.RetryInterval;
                }
            }
            else
            {
                nextEmailTemplateToSend = GetNextEmailTemplateToSend(lastEmailTempalte!);
            }

            // All emails in the schedule are sent for the given customer.
            if (nextEmailTemplateToSend is null)
            {
                var customerSchedule = dbContext.CustomerEmailSchedules!.Where(c => c.Id == schedule.Id).FirstOrDefault();
                customerSchedule!.Status = ScheduleStatus.Completed;
                await dbContext.SaveChangesAsync();

                break;
            }

            var nextExecutionTime = GetNextExecutionTime(schedule.Schedule!.Schedule, schedule.Customer!.Timezone, retryDelay, lastEmailLog);
            // check IsRightTimeToExecute()
            bool executeNow = IsRightTimeToExecute(nextExecutionTime);

            if (executeNow)
            {
                await emailFromTemplateService.SendToCustomerAsync(schedule.CustomerId, nextEmailTemplateToSend!.Name, GetTemplateArguments(), null, schedule.ScheduleId);
            }
        }

        return true;
    }

    private EmailTemplate? GetNextEmailTemplateToSend(EmailTemplate lastEmailTempalte)
    {
        var emailsOfGroup = dbContext.EmailTemplates!.Where(t => t.GroupId == lastEmailTempalte!.GroupId).ToList();

        var indexOfLastEmail = emailsOfGroup.IndexOf(lastEmailTempalte);

        if (emailsOfGroup.Count == indexOfLastEmail + 1)
        {
            return null;
        }

        var nextEmailToSend = emailsOfGroup[indexOfLastEmail + 1];

        return nextEmailToSend;
    }

    private Dictionary<string, string> GetTemplateArguments()
    {
        throw new NotImplementedException();
    }

    private bool IsRightTimeToExecute(DateTime nextExecutionTime)
    {
        if (nextExecutionTime <= DateTime.UtcNow)
        {
            return true;
        }

        return false;
    }

    private DateTime GetNextExecutionTime(string schedule, int? timezone, int retryDelay, EmailLog lastEmailLog)
    {
        throw new NotImplementedException();
    }
}
