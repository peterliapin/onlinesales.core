// <copyright file="ESOnlyQueryProviderFactory.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Web;
using Microsoft.Extensions.Options;
using OnlineSales.Configuration;
using OnlineSales.Data;
using OnlineSales.Entities;
using OnlineSales.Interfaces;

namespace OnlineSales.Infrastructure
{
    public class ESOnlyQueryProviderFactory<T> : QueryProviderFactory<T>
        where T : BaseEntityWithId, new()
    {
        public ESOnlyQueryProviderFactory(PgDbContext dbContext, EsDbContext esDbContext, IOptions<ApiSettingsConfig> apiSettingsConfig, IHttpContextHelper? httpContextHelper)
            : base(dbContext, esDbContext, apiSettingsConfig, httpContextHelper)
        {
        }

        public override IQueryProvider<T> BuildQueryProvider(int limit = -1)
        {
            var queryCommands = QueryStringParser.Parse(httpContextHelper.Request.QueryString.HasValue ? HttpUtility.UrlDecode(httpContextHelper.Request.QueryString.ToString()) : string.Empty);

            var queryBuilder = new QueryModelBuilder<T>(queryCommands, limit == -1 ? apiSettingsConfig.Value.MaxListSize : limit, dbContext);

            var indexPrefix = dbContext.Configuration.GetSection("Elastic:IndexPrefix").Get<string>();
            return new ESQueryProvider<T>(elasticClient, queryBuilder, indexPrefix!);
        }
    }
}
