// <copyright file="CustomerScheduledEmail.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Nest;
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
        try
        {
            // Load all the customer schedules in CustomerEmailSchedule table by pending status
            var schedules = dbContext.CustomerEmailSchedules!
                .Include(c => c.Schedule)
                .Include(c => c.Customer)
                .Where(s => s.Status == ScheduleStatus.Pending).ToList();

            foreach (var schedule in schedules)
            {
                EmailTemplate? nextEmailTemplateToSend;
                int retryDelay = 0;

                var lastEmailLog = dbContext.EmailLogs!.Where(
                        e => e.ScheduleId == schedule.ScheduleId &&
                        e.CustomerId == schedule.CustomerId).OrderByDescending(x => x.CreatedAt).FirstOrDefault();

                var lastEmailTempalte = lastEmailLog is not null ? dbContext.EmailTemplates!.Where(
                        e => e.Id == lastEmailLog!.TemplateId).FirstOrDefault() : null;

                // Retry logic if the last email is not sent
                if (lastEmailLog is not null && lastEmailLog!.Status is EmailStatus.NotSent)
                {
                    var emailNotSentCount = dbContext.EmailLogs!.Where(
                            e => e.ScheduleId == schedule.ScheduleId
                            && e.CustomerId == schedule.CustomerId
                            && e.TemplateId == lastEmailLog.TemplateId
                            && e.Status == EmailStatus.NotSent).Count();

                    // If all retry attempts completed, get the next email template to send.
                    if (emailNotSentCount > lastEmailTempalte!.RetryCount)
                    {
                        nextEmailTemplateToSend = GetNextEmailTemplateToSend(lastEmailTempalte, schedule.Schedule!.GroupId);
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
                    nextEmailTemplateToSend = GetNextEmailTemplateToSend(lastEmailTempalte!, schedule.Schedule!.GroupId);
                }

                // All emails in the schedule are sent for the given customer.
                if (nextEmailTemplateToSend is null)
                {
                    var customerSchedule = dbContext.CustomerEmailSchedules!.Where(c => c.Id == schedule.Id).FirstOrDefault();
                    customerSchedule!.Status = ScheduleStatus.Completed;
                    await dbContext.SaveChangesAsync();

                    break;
                }

                var nextExecutionTime = GetNextExecutionTime(schedule, retryDelay, lastEmailLog);

                if (nextExecutionTime is not null)
                {
                    // check IsRightTimeToExecute()
                    bool executeNow = IsRightTimeToExecute(nextExecutionTime.Value);

                    if (executeNow)
                    {
                        await emailFromTemplateService.SendToCustomerAsync(schedule.CustomerId, nextEmailTemplateToSend!.Name, GetTemplateArguments(), null, schedule.ScheduleId);
                    }
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"Error occurred when executing customer scheduled task in task runner {currentJob.Id}");
            return false;
        }
    }

    private EmailTemplate? GetNextEmailTemplateToSend(EmailTemplate lastEmailTempalte, int groupId)
    {
        var emailsOfGroup = dbContext.EmailTemplates!.Where(t => t.GroupId == groupId).OrderBy(t => t.Id).ToList();

        var indexOfLastEmail = lastEmailTempalte is not null ? emailsOfGroup.IndexOf(lastEmailTempalte) : -1;

        if (emailsOfGroup.Count == indexOfLastEmail + 1)
        {
            return null;
        }

        var nextEmailToSend = emailsOfGroup[indexOfLastEmail + 1];

        return nextEmailToSend;
    }

    private Dictionary<string, string> GetTemplateArguments()
    {
        // TODO: customer based template arguments
        // Get related variable dictionary from variableservice.
        // Add any required scope based variables into the dictionary.
        return new Dictionary<string, string> { { "Key", "Value" } };
    }

    private bool IsRightTimeToExecute(DateTime nextExecutionTime)
    {
        if (nextExecutionTime <= DateTime.UtcNow)
        {
            return true;
        }

        return false;
    }

    private DateTime? GetNextExecutionTime(CustomerEmailSchedule schedule, int retryDelay, EmailLog? lastEmailLog)
    {
        Schedule? customerSchedule = JsonSerializer.Deserialize<Schedule>(schedule.Schedule!.Schedule);
        var userToServerTimeZoneOffset = TimeZoneInfo.Local.BaseUtcOffset.TotalMinutes + schedule.Customer!.Timezone!.Value;
        var lastRunTime = lastEmailLog is null ? schedule.Customer!.CreatedAt : lastEmailLog.CreatedAt;

        // If a retry scenario, adding the retry interval. No need to evaluate schedule.
        if (retryDelay > 0)
        {
            return DateTime.SpecifyKind(lastRunTime.AddMinutes(retryDelay), DateTimeKind.Utc);
        }

        // Evaluate CRON based schedule
        if (!string.IsNullOrEmpty(customerSchedule!.Cron))
        {
            Quartz.CronExpression expression = new Quartz.CronExpression(customerSchedule.Cron);

            var nextRunTimeForUser = expression.GetNextValidTimeAfter(lastRunTime.AddMinutes(-userToServerTimeZoneOffset));
            var nextRunTime = nextRunTimeForUser!.Value.AddMinutes(userToServerTimeZoneOffset);

            return DateTime.SpecifyKind(nextRunTime.DateTime, DateTimeKind.Utc);
        }
        else
        {
            // Evaluate custom scheudle based on day and time.

            var days = customerSchedule.Day!.Split(',').Select(int.Parse).ToArray();

            var emailSentCount = dbContext.EmailLogs!.Where(
                            e => e.ScheduleId == schedule.ScheduleId
                            && e.CustomerId == schedule.CustomerId
                            && e.Status == EmailStatus.Sent).Count();

            // Skip the days already the mail is sent 
            var nextRunDate = schedule.Customer!.CreatedAt.AddDays(days[emailSentCount]);
            // Add given time in the schedule + user timezone adjustment.
            var nextRunDateTime = DateOnly.FromDateTime(nextRunDate).ToDateTime(customerSchedule!.Time!.Value).AddMinutes(userToServerTimeZoneOffset);

            return DateTime.SpecifyKind(nextRunDateTime, DateTimeKind.Utc);
        }
    }
}

public class Schedule
{
    public string? Cron { get; set; } = string.Empty;

    public string? Day { get; set; } = string.Empty;

    public TimeOnly? Time { get; set; }
}