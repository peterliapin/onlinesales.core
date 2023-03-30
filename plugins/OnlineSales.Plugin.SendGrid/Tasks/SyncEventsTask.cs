// <copyright file="SyncEventsTask.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using Microsoft.Extensions.Configuration;
using OnlineSales.Configuration;
using OnlineSales.Data;
using OnlineSales.DataAnnotations;
using OnlineSales.DTOs;
using OnlineSales.Entities;
using OnlineSales.Helpers;
using OnlineSales.Plugin.SendGrid.Data;
using OnlineSales.Plugin.SendGrid.Entities;
using OnlineSales.Plugin.SendGrid.Exceptions;
using OnlineSales.Services;
using OnlineSales.Tasks;

namespace OnlineSales.SendGrid.Tasks
{
    public class SyncEventsTask : ChangeLogTask
    {
        private readonly SendgridDbContext sgDbContext;
        private readonly ActivityLogService logService;

        public SyncEventsTask(IConfiguration configuration, SendgridDbContext dbContext, IEnumerable<PluginDbContextBase> pluginDbContexts, TaskStatusService taskStatusService, ActivityLogService logService)
            : base("Tasks:SyncEventsTask", configuration, dbContext, pluginDbContexts, taskStatusService)
        {
            this.sgDbContext = dbContext;
            this.logService = logService;
        }

        protected async override void ExecuteLogTask(List<ChangeLog> nextBatch)
        {
            var ids = nextBatch.Select(i => i.ObjectId);
            if (ids.Any())
            {
                // we use the fact that records in SendgridEvents table can be just added not removed
                var min = ids.Min();
                var max = ids.Max();
                var events = sgDbContext.SendgridEvents !.Where(e => e.Id >= min && e.Id <= max).Select(e => Convert(e)).ToList();
                                
                var res = await logService.AddActivityRecords(events);
                if (!res)
                {
                    throw new SendGridApiException("Cannot log sendgrid events");                        
                }
            }            
        }

        protected override bool IsTypeSupported(Type type)
        {
            return type == typeof(SendgridEvent);
        }

        private static ActivityLogDto Convert(SendgridEvent ev)
        {
            return new ActivityLogDto()
            {
                Source = "SendGrid",
                Type = ev.Event,
                CreatedAt = ev.CreatedAt,
                ContactId = ev.ContactId,
                Ip = ev.Ip,
                Data = JsonHelper.Serialize(new { Id = ev.Id, MessageId = ev.MessageId, EventId = ev.EventId }),
            };
        }
    }
}
