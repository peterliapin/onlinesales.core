// <copyright file="ActivityLogService.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Configuration;
using Nest;
using OnlineSales.Data;
using OnlineSales.DTOs;

namespace OnlineSales.Services
{
    public class ActivityLogService
    {
        private readonly string indexName;

        private readonly EsDbContext esDbContext;

        public ActivityLogService(IConfiguration configuration, EsDbContext esDbContext)
        {
            indexName = configuration.GetSection("Elastic:IndexPrefix").Get<string>() + "-activitylog";
            this.esDbContext = esDbContext;
        }

        public async Task<bool> AddActivityRecords(List<ActivityLogDto> records)
        {
            if (!esDbContext.ElasticClient.Indices.Exists(indexName).Exists)
            {
                esDbContext.ElasticClient.Indices.Create(indexName, index => index.Map<ActivityLogDto>(x => x.AutoMap()));
            }

            var responce = await esDbContext.ElasticClient.IndexManyAsync<ActivityLogDto>(records, indexName);

            if (!responce.IsValid)
            {
                Log.Error("Cannot save logs in Elastic Search. Reason: " + responce.DebugInformation);
            }            

            return responce.IsValid;
        }
    }
}