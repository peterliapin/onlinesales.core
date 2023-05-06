// <copyright file="SyncActivityLogTask.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using Microsoft.Extensions.Configuration;
using OnlineSales.Configuration;
using OnlineSales.Data;
using OnlineSales.DataAnnotations;
using OnlineSales.DTOs;
using OnlineSales.Entities;
using OnlineSales.Exceptions;
using OnlineSales.Helpers;
using OnlineSales.Plugin.SendGrid.Data;
using OnlineSales.Plugin.SendGrid.Entities;
using OnlineSales.Plugin.SendGrid.Exceptions;
using OnlineSales.Services;
using OnlineSales.Tasks;
using Serilog;

namespace OnlineSales.SendGrid.Tasks
{
    public class SyncActivityLogTask : BaseTask
    {
        private static readonly string SourceName = "SendGrid";

        private readonly SendgridDbContext sgDbContext;

        private readonly ActivityLogService logService;

        private readonly int batchSize;

        public SyncActivityLogTask(IConfiguration configuration, SendgridDbContext dbContext, TaskStatusService taskStatusService, ActivityLogService logService)
            : base("Tasks:SyncActivityLogTask", configuration, taskStatusService)
        {
            sgDbContext = dbContext;
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
                var events = sgDbContext.SendgridEvents!.Where(e => e.Id > maxId).OrderBy(e => e.Id).Take(batchSize).Select(e => Convert(e)).ToList();
                var res = await logService.AddActivityRecords(events);
                if (!res)
                {
                    throw new SendGridApiException("Cannot log sendgrid events");
                }

                return true;
            }
            catch (Exception ex)
            {
                Log.Error("Error while logging sendgrid events. Reason: " + ex.Message);
                throw;
            }
        }

        private static ActivityLogDto Convert(SendgridEvent ev)
        {
            return new ActivityLogDto()
            {
                Source = SourceName,
                SourceId = ev.Id,
                Type = ev.Event,
                CreatedAt = ev.CreatedAt,
                ContactId = ev.ContactId,
                Ip = ev.Ip,
                Data = JsonHelper.Serialize(new { Id = ev.Id, MessageId = ev.MessageId, EventId = ev.EventId }),
            };
        }
    }
}