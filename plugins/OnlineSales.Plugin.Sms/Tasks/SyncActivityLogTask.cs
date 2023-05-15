// <copyright file="SyncActivityLogTask.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using Microsoft.Extensions.Configuration;
using OnlineSales.Configuration;
using OnlineSales.DTOs;
using OnlineSales.Entities;
using OnlineSales.Exceptions;
using OnlineSales.Helpers;
using OnlineSales.Plugin.Sms.Data;
using OnlineSales.Plugin.Sms.Entities;
using OnlineSales.Plugin.Sms.Exceptions;
using OnlineSales.Services;
using OnlineSales.Tasks;
using Serilog;

namespace OnlineSales.SendGrid.Tasks
{
    public class SyncActivityLogTask : BaseTask
    {
        private static readonly string SourceName = "SMS";

        private readonly SmsDbContext dbContext;

        private readonly ActivityLogService logService;

        private readonly int batchSize;

        public SyncActivityLogTask(IConfiguration configuration, SmsDbContext dbContext, TaskStatusService taskStatusService, ActivityLogService logService)
            : base("Tasks:SyncActivityLogTask", configuration, taskStatusService)
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
                var events = dbContext.SmsLogs!.Where(e => e.Id > maxId).OrderBy(e => e.Id).Take(batchSize).Select(e => Convert(e)).ToList();
                var res = await logService.AddActivityRecords(events);
                if (!res)
                {
                    throw new SmsPluginException("Unable to log sms plugin events");
                }

                return true;
            }
            catch (Exception ex)
            {
                Log.Error("Failed to dump sms plugin events to activity log. Reason: " + ex.Message);
                throw;
            }
        }

        private static ActivityLogDto Convert(SmsLog ev)
        {
            return new ActivityLogDto()
            {
                Source = SourceName,
                SourceId = ev.Id,
                Type = "Message",
                CreatedAt = ev.CreatedAt,
                Data = JsonHelper.Serialize(new { Id = ev.Id, Status = ev.Status, Sender = ev.Sender, Recipient = ev.Recipient }),
            };
        }
    }
}