// <copyright file="QueryFactory.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Web;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Nest;
using OnlineSales.Configuration;
using OnlineSales.Data;
using OnlineSales.DataAnnotations;
using OnlineSales.Entities;
using OnlineSales.Infrastructure;
using OnlineSales.Interfaces;

namespace OnlineSales.Services
{
    public class QueryFactory<T>
        where T : BaseEntityWithId, new()
    {
        private readonly DbSet<T> dbSet;
        private readonly PgDbContext dbContext;
        private readonly IOptions<ApiSettingsConfig> apiSettingsConfig;
        private readonly ElasticClient elasticClient;
        private readonly IHttpContextHelper httpContextHelper;

        public QueryFactory(PgDbContext dbContext, EsDbContext esDbContext, IOptions<ApiSettingsConfig> apiSettingsConfig, IHttpContextHelper? httpContextHelper)
        {
            this.dbContext = dbContext;
            this.apiSettingsConfig = apiSettingsConfig;

            dbSet = dbContext.Set<T>();
            elasticClient = esDbContext.ElasticClient;

            ArgumentNullException.ThrowIfNull(httpContextHelper);
            this.httpContextHelper = httpContextHelper;
        }

        public IQueryProvider<T> BuildQueryProvider(int limit = -1)
        {
            var queryCommands = QueryParser.Parse(httpContextHelper.Request.QueryString.HasValue ? HttpUtility.UrlDecode(httpContextHelper.Request.QueryString.ToString()) : string.Empty);

            var queryData = new QueryData<T>(queryCommands, limit == -1 ? apiSettingsConfig.Value.MaxListSize : limit, dbContext);

            if (typeof(T).GetCustomAttributes(typeof(SupportsElasticAttribute), true).Any() && queryData.SearchData.Count > 0)
            {
                var indexPrefix = dbContext.Configuration.GetSection("Elastic:IndexPrefix").Get<string>();
                return new MixedQueryProvider<T>(queryData, dbSet!.AsQueryable<T>(), elasticClient, indexPrefix!);
            }
            else
            {
                return new DBQueryProvider<T>(dbSet!.AsQueryable<T>(), queryData);
            }
        }
    }
}
