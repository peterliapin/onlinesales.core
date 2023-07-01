// <copyright file="SyncEsTask.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Reflection;
using System.Text;
using Elasticsearch.Net;
using Microsoft.EntityFrameworkCore;
using Nest;
using OnlineSales.Data;
using OnlineSales.DataAnnotations;
using OnlineSales.Entities;
using OnlineSales.Helpers;
using OnlineSales.Services;

namespace OnlineSales.Tasks
{
    public class SyncEsTask : ChangeLogTask
    {
        private readonly string changeLogId = "change_log_id";
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
                    var createItem = new { index = new { _index = GetIndexName(item.ObjectType), _id = item.ObjectId } };
                    var data = JsonHelper.Deserialize<Dictionary<string, object>>(item.Data);
                    data!.Add(changeLogId, item.Id);
                    bulkPayload.AppendLine(JsonHelper.Serialize(createItem));
                    bulkPayload.AppendLine(JsonHelper.Serialize(data));
                }

                if (entityState == EntityState.Deleted)
                {
                    var deleteItem = new { delete = new { _index = GetIndexName(item.ObjectType), _id = item.ObjectId } };
                    bulkPayload.AppendLine(JsonHelper.Serialize(deleteItem));
                }
            }

            var bulkRequestParameters = new BulkRequestParameters();
            bulkRequestParameters.Refresh = Refresh.WaitFor;

            var bulkResponse = esDbContext.ElasticClient.LowLevel.Bulk<StringResponse>(bulkPayload.ToString(), bulkRequestParameters);

            if (!bulkResponse.Success)
            {
                throw new ESSyncTaskException("Cannot write bulk data into elastic");
            }

            Log.Information("ES Sync Bulk Saved : {0}", bulkResponse.ToString());
        }

        protected override int GetMinLogId(ChangeLogTaskLog lastProcessedTask, Type loggedType)
        {
            var minLogId = 1;

            if (esDbContext.ElasticClient.Indices.Exists(GetIndexName(loggedType.Name)).Exists)
            {
                var requestResponse = esDbContext.ElasticClient.Search<Dictionary<string, object>>(s => s.Query(q => new MatchAllQuery { })
                .Index(GetIndexName(loggedType.Name))
                .Sort(s => s.Descending(d => d[changeLogId]))
                .Size(1));
                if (requestResponse.IsValid && requestResponse.Documents.Count > 0)
                {
                    minLogId = (int)((long)requestResponse.Documents.First()[changeLogId] + 1);
                }
            }

            return minLogId;
        }

        protected override bool IsTypeSupported(Type type)
        {
            return type.GetCustomAttribute<SupportsElasticAttribute>() != null;
        }

        private string GetIndexName(string loggedTypeName)
        {
            return prefix + loggedTypeName.ToLower();
        }

        public class ESSyncTaskException : Exception
        {
            public ESSyncTaskException(string message)
                : base(message)
            {
            }
        }
    }
}