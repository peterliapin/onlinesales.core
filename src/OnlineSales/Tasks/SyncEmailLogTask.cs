// <copyright file="SyncEmailLogTask.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OnlineSales.Configuration;
using OnlineSales.Data;
using OnlineSales.DTOs;
using OnlineSales.Entities;
using OnlineSales.Exceptions;
using OnlineSales.Helpers;
using OnlineSales.Services;
using OnlineSales.Tasks;
using Serilog;

namespace OnlineSales.Tasks
{
    public class SyncEmailLogTask : BaseTask
    {
        private static readonly string SourceName = "EmailService";
        private readonly PgDbContext dbContext;
        private readonly ActivityLogService logService;

        private readonly int batchSize;

        public SyncEmailLogTask(IConfiguration configuration, PgDbContext dbContext, TaskStatusService taskStatusService, ActivityLogService logService)
            : base("Tasks:SyncEmailLogTask", configuration, taskStatusService)
        {
            this.dbContext = dbContext;
            this.logService = logService;
            var config = configuration.GetSection(configKey)!.Get<TaskWithBatchConfig>();
            if (config is not null)
            {
                batchSize = config.BatchSize;
            }
            else
            {
                throw new MissingConfigurationException($"The specified configuration section for the provided configKey {configKey} could not be found in the settings file.");
            }
        }

        public async override Task<bool> Execute(TaskExecutionLog currentJob)
        {
            try
            {
                var maxId = await logService.GetMaxId(SourceName);
                var batch = await dbContext.EmailLogs!.Where(e => e.Id > maxId).OrderBy(e => e.Id).Take(batchSize).ToArrayAsync();
                var tasks = batch.Select(Convert).ToArray();
                Task.WaitAll(tasks);
                await dbContext.SaveChangesAsync();
                var res = await logService.AddActivityRecords(tasks.Select(t => t.Result).ToList());
                if (!res)
                {
                    throw new SyncEmailLogTaskException("Unable to log email events");
                }

                return true;
            }
            catch (Exception ex)
            {
                Log.Error("Failed to dump email events to activity log. Reason: " + ex.Message);
                throw;
            }
        }

        private async Task<ActivityLog> Convert(EmailLog ev)
        {
            var contact = await dbContext.Contacts!.FirstOrDefaultAsync(contact => contact.Email == ev.Recipient);
            if (contact == null)
            {
                contact = (await dbContext.Contacts!.AddAsync(new Contact
                {
                    Email = ev.Recipient,
                })).Entity;
            }

            return new ActivityLog()
            {
                Source = SourceName,
                SourceId = ev.Id,
                Type = "Message",
                ContactId = contact.Id,
                CreatedAt = ev.CreatedAt,
                Data = JsonHelper.Serialize(new { Id = ev.Id, Status = ev.Status, Sender = ev.FromEmail, Recipient = ev.Recipient, Subject = ev.Subject }),
            };
        }
    }
}