// <copyright file="SyncEsTask.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Configuration;
using System.Text;
using System.Text.Json;
using Elasticsearch.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Nest;
using OnlineSales.Configuration;
using OnlineSales.Data;
using OnlineSales.Entities;

namespace OnlineSales.Tasks
{
    public class SyncEsTask : ChangeLogTask
    {
        private readonly TaskConfig? taskConfig = new TaskConfig();
        private readonly ElasticClient elasticClient;
        private readonly string prefix = string.Empty;

        public SyncEsTask(IConfiguration configuration, ApiDbContext dbContext, ElasticClient elasticClient)
            : base(dbContext, configuration)
        {
            var config = configuration.GetSection("Tasks:SyncEsTask") !.Get<TaskConfig>();
            if (config is not null)
            {
                taskConfig = config;
            }

            var configPrefix = configuration.GetSection("Elasticsearch:IndexPrefix").Get<string>();

            if (!string.IsNullOrEmpty(configPrefix))
            {
                prefix = configPrefix + "-";
            }

            this.elasticClient = elasticClient;
        }

        public override string Name => "SyncEsTask";

        public override string CronSchedule => taskConfig!.CronSchedule;

        public override int RetryCount => taskConfig!.RetryCount;

        public override int RetryInterval => taskConfig!.RetryInterval;

        internal override void ExecuteLogTask(List<ChangeLog> nextBatch)
        {
            var bulkPayload = new StringBuilder();
            
            foreach (var item in nextBatch)
            {
                EntityState entityState = item.EntityState;

                if (entityState == EntityState.Added)
                {
                    var createItem = new { index = new { _index = prefix + item.ObjectType.ToLower(), _id = item.ObjectId } };
                    bulkPayload.AppendLine(JsonSerializer.Serialize(createItem));
                    bulkPayload.AppendLine(item.Data);
                }

                if (entityState == EntityState.Deleted)
                {
                    var deleteItem = new { delete = new { _index = prefix + item.ObjectType.ToLower(), _id = item.ObjectId } };
                    bulkPayload.AppendLine(JsonSerializer.Serialize(deleteItem));
                }
            }

            var bulkResponse = elasticClient.LowLevel.Bulk<StringResponse>(bulkPayload.ToString());

            Log.Information("ES Sync Bulk Saved : {0}", bulkResponse.ToString());
        }
    }
}
