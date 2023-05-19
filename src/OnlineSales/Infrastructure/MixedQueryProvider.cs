// <copyright file="MixedQueryProvider.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Reflection;
using Nest;
using OnlineSales.Data;
using OnlineSales.Entities;

namespace OnlineSales.Infrastructure
{
    public class MixedQueryProvider<T> : IQueryProvider<T>
        where T : BaseEntityWithId
    {
        private readonly QueryData<T> parseData;

        private readonly ElasticClient elasticClient;

        private readonly string indexPrefix;

        private IQueryable<T> query;

        public MixedQueryProvider(QueryData<T> parseData, IQueryable<T> query, ElasticClient elasticClient, string indexPrefix/*, PgDbContext dbContext*/)
        {
            this.parseData = parseData;
            this.elasticClient = elasticClient;
            this.indexPrefix = indexPrefix;
            this.query = query;
        }

        public async Task<QueryResult<T>> GetResult()
        {
            var selectData = new QueryData<T>.SelectCommandData();
            selectData.SelectedProperties.AddRange(parseData.SelectData.SelectedProperties);
            selectData.IsSelect = parseData.SelectData.IsSelect;
            var includeData = new List<PropertyInfo>();
            includeData.AddRange(parseData.IncludeData);

            var idSelectData = new QueryData<T>.SelectCommandData() { SelectedProperties = new List<PropertyInfo> { typeof(T).GetProperty("Id") ! }, IsSelect = true };

            parseData.SelectData = idSelectData;
            parseData.IncludeData.Clear();

            var esProvider = new ESQueryProvider<T>(elasticClient, parseData, indexPrefix);
            var esResult = await esProvider.GetResult();
            List<int> ids = esResult.Records == null ? new List<int>() : esResult.Records.Select(r => r.Id).ToList();

            query = query.Where(e => ids.Contains(e.Id));

            parseData.WhereData.Clear();
            parseData.SearchData.Clear();
            parseData.IncludeData = includeData;
            parseData.SelectData = selectData;
            parseData.Skip = 0;
            parseData.Limit = ids.Count;

            var dbProvider = new DBQueryProvider<T>(query, parseData);

            var dbResult = await dbProvider.GetResult();

            return new QueryResult<T>(dbResult.Records, esResult.TotalCount);
        }
    }
}
