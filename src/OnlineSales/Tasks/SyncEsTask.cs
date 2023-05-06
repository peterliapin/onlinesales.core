// <copyright file="SyncEsTask.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Reflection;
using System.Text;
using Elasticsearch.Net;
using Microsoft.EntityFrameworkCore;
using OnlineSales.Data;
using OnlineSales.DataAnnotations;
using OnlineSales.Entities;
using OnlineSales.Helpers;
using OnlineSales.Services;

namespace OnlineSales.Tasks
{
    public class SyncEsTask : ChangeLogTask
    {
        private readonly EsDbContext esDbContext;
        private readonly string prefix = string.Empty;

        public SyncEsTask(IConfiguration configuration, PgDbContext dbContext, IEnumerable<PluginDbContextBase> pluginDbContexts, EsDbContext esDbContext, TaskStatusService taskStatusService)
            : base("Tasks:SyncEsTask", configuration, dbContext, pluginDbContexts, taskStatusService)
        {
            var elasticPrefix = configuration.GetSection("Elastic:IndexPrefix").Get<string>();

            if (!string.IsNullOrEmpty(elasticPrefix))
            {
                prefix = elasticPrefix + "-";
            }

            this.esDbContext = esDbContext;
        }

        protected override void ExecuteLogTask(List<ChangeLog> nextBatch)
        {
            var bulkPayload = new StringBuilder();

            foreach (var item in nextBatch)
            {
                var entityState = item.EntityState;

                if (entityState == EntityState.Added || entityState == EntityState.Modified)
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

            var bulkRequestParameters = new BulkRequestParameters();
            bulkRequestParameters.Refresh = Refresh.WaitFor;

            var bulkResponse = esDbContext.ElasticClient.LowLevel.Bulk<StringResponse>(bulkPayload.ToString(), bulkRequestParameters);

            Log.Information("ES Sync Bulk Saved : {0}", bulkResponse.ToString());
        }

        protected override bool IsTypeSupported(Type type)
        {
            return type.GetCustomAttribute<SupportsElasticAttribute>() != null;
        }
    }
}