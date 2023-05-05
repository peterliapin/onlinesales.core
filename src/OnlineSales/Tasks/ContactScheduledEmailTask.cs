// <copyright file="ContactScheduledEmailTask.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using OnlineSales.Data;
using OnlineSales.Entities;
using OnlineSales.Interfaces;
using OnlineSales.Services;

namespace OnlineSales.Tasks;

public class ContactScheduledEmailTask : BaseTask
{
    private readonly PgDbContext dbContext;
    private readonly IEmailFromTemplateService emailFromTemplateService;

    public ContactScheduledEmailTask(PgDbContext dbContext, IEmailFromTemplateService emailFromTemplateService, IConfiguration configuration, TaskStatusService taskStatusService)
        : base("Tasks:ContactScheduledEmail", configuration, taskStatusService)
    {
        this.dbContext = dbContext;
        this.emailFromTemplateService = emailFromTemplateService;
    }

    public override async Task<bool> Execute(TaskExecutionLog currentJob)
    {
        try
        {
            // Load all the contact schedules in ContactEmailSchedule table by pending status
            var schedules = this.dbContext.ContactEmailSchedules!
                .Include(c => c.Schedule)
                .Include(c => c.Contact)
                .Where(s => s.Status == ScheduleStatus.Pending).ToList();

            foreach (var schedule in schedules)
            {
                EmailTemplate? nextEmailTemplateToSend;
                var retryDelay = 0;

                var lastEmailLog = this.dbContext.EmailLogs!.Where(
                        e => e.ScheduleId == schedule.ScheduleId &&
                        e.ContactId == schedule.ContactId).OrderByDescending(x => x.CreatedAt).FirstOrDefault();

                var lastEmailTemplate = lastEmailLog is not null
                    ? this.dbContext.EmailTemplates!.FirstOrDefault(e => e.Id == lastEmailLog!.TemplateId)
                    : null;

                // Retry logic if the last email is not sent
                if (lastEmailLog is not null && lastEmailLog!.Status is EmailStatus.NotSent)
                {
                    var emailNotSentCount = this.dbContext.EmailLogs!.Count(
                            e => e.ScheduleId == schedule.ScheduleId
                            && e.ContactId == schedule.ContactId
                            && e.TemplateId == lastEmailLog.TemplateId
                            && e.Status == EmailStatus.NotSent);

                    // If all retry attempts completed, get the next email template to send.
                    if (emailNotSentCount > lastEmailTemplate!.RetryCount)
                    {
                        nextEmailTemplateToSend = this.GetNextEmailTemplateToSend(lastEmailTemplate, schedule.Schedule!.GroupId);
                    }
                    else
                    {
                        // Retry attempt available. sending the same email template.
                        nextEmailTemplateToSend = this.dbContext.EmailTemplates!.FirstOrDefault(t => t.Id == lastEmailTemplate.Id);
                        retryDelay = lastEmailTemplate!.RetryInterval;
                    }
                }
                else
                {
                    nextEmailTemplateToSend = this.GetNextEmailTemplateToSend(lastEmailTemplate!, schedule.Schedule!.GroupId);
                }

                // All emails in the schedule are sent for the given contact.
                if (nextEmailTemplateToSend is null)
                {
                    var contactSchedule = this.dbContext.ContactEmailSchedules!.FirstOrDefault(c => c.Id == schedule.Id);
                    contactSchedule!.Status = ScheduleStatus.Completed;
                    await this.dbContext.SaveChangesAsync();

                    break;
                }

                var nextExecutionTime = this.GetNextExecutionTime(schedule, retryDelay, lastEmailLog);

                if (nextExecutionTime is not null)
                {
                    // check IsRightTimeToExecute()
                    var executeNow = this.IsRightTimeToExecute(nextExecutionTime.Value);

                    if (executeNow)
                    {
                        await this.emailFromTemplateService.SendToContactAsync(schedule.ContactId, nextEmailTemplateToSend!.Name, this.GetTemplateArguments(), null, schedule.ScheduleId);
                    }
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"Error occurred when executing contact scheduled task in task runner {currentJob.Id}");
            return false;
        }
    }

    private EmailTemplate? GetNextEmailTemplateToSend(EmailTemplate lastEmailTemplate, int groupId)
    {
        var emailsOfGroup = this.dbContext.EmailTemplates!.Where(t => t.EmailGroupId == groupId).OrderBy(t => t.Id).ToList();

        var indexOfLastEmail = lastEmailTemplate is not null
            ? emailsOfGroup.IndexOf(lastEmailTemplate)
            : -1;

        if (emailsOfGroup.Count == indexOfLastEmail + 1)
        {
            return null;
        }

        var nextEmailToSend = emailsOfGroup[indexOfLastEmail + 1];

        return nextEmailToSend;
    }

    private Dictionary<string, string> GetTemplateArguments()
    {
        // TODO: contact based template arguments
        // Get related variable dictionary from variable service.
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

    private DateTime? GetNextExecutionTime(ContactEmailSchedule schedule, int retryDelay, EmailLog? lastEmailLog)
    {
        var contactSchedule = JsonSerializer.Deserialize<Schedule>(schedule.Schedule!.Schedule);
        var userToServerTimeZoneOffset = TimeZoneInfo.Local.BaseUtcOffset.TotalMinutes + schedule.Contact!.Timezone!.Value;
        var lastRunTime = lastEmailLog is null ? schedule.Contact!.CreatedAt : lastEmailLog.CreatedAt;

        // If a retry scenario, adding the retry interval. No need to evaluate schedule.
        if (retryDelay > 0)
        {
            return DateTime.SpecifyKind(lastRunTime.AddMinutes(retryDelay), DateTimeKind.Utc);
        }

        // Evaluate CRON based schedule
        if (!string.IsNullOrEmpty(contactSchedule!.Cron))
        {
            var expression = new Quartz.CronExpression(contactSchedule.Cron);

            var nextRunTimeForUser = expression.GetNextValidTimeAfter(lastRunTime.AddMinutes(-userToServerTimeZoneOffset));
            var nextRunTime = nextRunTimeForUser!.Value.AddMinutes(userToServerTimeZoneOffset);

            return DateTime.SpecifyKind(nextRunTime.DateTime, DateTimeKind.Utc);
        }
        else
        {
            // Evaluate custom scheudle based on day and time.

            var days = contactSchedule.Day!.Split(',').Select(int.Parse).ToArray();

            var emailSentCount = this.dbContext.EmailLogs!.Count(
                            e => e.ScheduleId == schedule.ScheduleId
                            && e.ContactId == schedule.ContactId
                            && e.Status == EmailStatus.Sent);

            // Skip the days already the mail is sent 
            var nextRunDate = schedule.Contact!.CreatedAt.AddDays(days[emailSentCount]);
            // Add given time in the schedule + user timezone adjustment.
            var nextRunDateTime = DateOnly.FromDateTime(nextRunDate).ToDateTime(contactSchedule!.Time!.Value).AddMinutes(userToServerTimeZoneOffset);

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