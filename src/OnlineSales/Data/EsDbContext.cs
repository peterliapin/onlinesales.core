// <copyright file="EsDbContext.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Nest;
using OnlineSales.Configuration;
using OnlineSales.DataAnnotations;
using OnlineSales.Elastic;
using OnlineSales.Entities;

namespace OnlineSales.Data;

public class EsDbContext : ElasticDbContext
{
    private readonly ElasticConfig? elasticConfig;

    private readonly ElasticClient elasticClient;

    private readonly List<Type> entityTypes;

    public EsDbContext(IConfiguration configuration)
    {
        elasticConfig = configuration.GetSection("Elastic").Get<ElasticConfig>();

        if (elasticConfig == null)
        {
            throw new MissingConfigurationException("Elastic configuration is mandatory.");
        }

        var connectionSettings = new ConnectionSettings(new Uri(elasticConfig.Url));

        connectionSettings.DefaultMappingFor<LogRecord>(m => m
            .IndexName($"{elasticConfig!.IndexPrefix}-logs"));

        var assembly = typeof(EsDbContext).Assembly;

        entityTypes =
            (from t in assembly.GetTypes().AsParallel()
             let attributes = t.GetCustomAttributes(typeof(SupportsElasticAttribute), true)
             where attributes != null && attributes.Length > 0
             select t).ToList();

        var migrationsIndexName = ElasticHelper.GetIndexName(elasticConfig!.IndexPrefix, typeof(ElasticMigration));

        connectionSettings.DefaultMappingFor<ElasticMigration>(m => m
            .IndexName(migrationsIndexName));

        foreach (var entityType in entityTypes)
        {
            var indexName = ElasticHelper.GetIndexName(elasticConfig!.IndexPrefix, entityType);

            connectionSettings.DefaultMappingFor(entityType, m => m
                .IndexName(indexName));
        }

        elasticClient = new ElasticClient(connectionSettings);
    }

    public override ElasticClient ElasticClient => elasticClient;

    public override string IndexPrefix => elasticConfig!.IndexPrefix;

    protected override List<Type> EntityTypes => entityTypes;
}