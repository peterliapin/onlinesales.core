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
        this.elasticConfig = configuration.GetSection("Elastic").Get<ElasticConfig>();

        if (this.elasticConfig == null)
        {
            throw new MissingConfigurationException("Elastic configuration is mandatory.");
        }

        var connectionSettings = new ConnectionSettings(new Uri(this.elasticConfig.Url));

        connectionSettings.DefaultMappingFor<LogRecord>(m => m
            .IndexName($"{this.elasticConfig!.IndexPrefix}-logs"));

        var assembly = typeof(EsDbContext).Assembly;

        this.entityTypes =
            (from t in assembly.GetTypes().AsParallel()
             let attributes = t.GetCustomAttributes(typeof(SupportsElasticAttribute), true)
             where attributes != null && attributes.Length > 0
             select t).ToList();

        var migrationsIndexName = ElasticHelper.GetIndexName(this.elasticConfig!.IndexPrefix, typeof(ElasticMigration));

        connectionSettings.DefaultMappingFor<ElasticMigration>(m => m
            .IndexName(migrationsIndexName));

        foreach (var entityType in this.entityTypes)
        {
            var indexName = ElasticHelper.GetIndexName(this.elasticConfig!.IndexPrefix, entityType);

            connectionSettings.DefaultMappingFor(entityType, m => m
                .IndexName(indexName));
        }

        this.elasticClient = new ElasticClient(connectionSettings);
    }

    public override ElasticClient ElasticClient => this.elasticClient;

    public override string IndexPrefix => this.elasticConfig!.IndexPrefix;

    protected override List<Type> EntityTypes => this.entityTypes;
}

