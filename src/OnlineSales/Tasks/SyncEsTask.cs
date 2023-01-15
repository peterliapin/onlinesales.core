// <copyright file="SyncEsTask.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Text;
using Elasticsearch.Net;
using Microsoft.EntityFrameworkCore;
using Nest;
using OnlineSales.Configuration;
using OnlineSales.Data;
using OnlineSales.Entities;
using OnlineSales.Helpers;

namespace OnlineSales.Tasks
{
    public class SyncEsTask : ChangeLogTask
    {
        private readonly TaskConfig? taskConfig = new TaskConfig();
        private readonly ElasticClient elasticClient;
        private readonly string prefix = string.Empty;

        public SyncEsTask(IConfiguration configuration, ApiDbContext dbContext, ElasticClient elasticClient)
            : base(dbContext)
        {
            var config = configuration.GetSection("Tasks:SyncEsTask") !.Get<TaskConfig>();
            if (config is not null)
            {
                taskConfig = config;
            }

            var elasticPrefix = configuration.GetSection("Elasticsearch:IndexPrefix").Get<string>();

            if (!string.IsNullOrEmpty(elasticPrefix))
            {
                prefix = elasticPrefix + "-";
            }

            this.elasticClient = elasticClient;
        }

        public override string CronSchedule => taskConfig!.CronSchedule;

        public override int RetryCount => taskConfig!.RetryCount;

        public override int RetryInterval => taskConfig!.RetryInterval;

        public override string[] Entities => new[] { "Customer", "Post" };

        internal override void ExecuteLogTask(List<ChangeLog> nextBatch)
        {
            var bulkPayload = new StringBuilder();
            
            foreach (var item in nextBatch)
            {
                EntityState entityState = item.EntityState;

                if (entityState == EntityState.Added)
                {
                    var createItem = new { index = new { _index = prefix + item.ObjectType.ToLower(), _id = item.ObjectId } };
                    bulkPayload.AppendLine(JsonHelper.Serialize(createItem));
                    bulkPayload.AppendLine(item.Data);
                }

                if (entityState == EntityState.Deleted)
                {
                    var deleteItem = new { delete = new { _index = prefix + item.ObjectType.ToLower(), _id = item.ObjectId } };
                    bulkPayload.AppendLine(JsonHelper.Serialize(deleteItem));
                }
            }

            var bulkResponse = elasticClient.LowLevel.Bulk<StringResponse>(bulkPayload.ToString());

            Log.Information("ES Sync Bulk Saved : {0}", bulkResponse.ToString());
        }
    }
}
