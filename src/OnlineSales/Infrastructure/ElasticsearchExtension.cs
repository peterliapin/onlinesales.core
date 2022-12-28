// <copyright file="ElasticsearchExtension.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using Nest;
using OnlineSales.Configuration;
using OnlineSales.Entities;

namespace OnlineSales.Infrastructure;

public static class ElasticsearchExtension
{
    public static void AddElasticsearch(this IServiceCollection services, ElasticsearchConfig elasticConfig)
    {
        var settings = new ConnectionSettings(new Uri(elasticConfig.Url));

        settings.DefaultMappingFor<LogRecord>(m => m
            .IndexName($"{elasticConfig.IndexPrefix}-logs"));

        var client = new ElasticClient(settings);

        services.AddSingleton(client);
    }
}