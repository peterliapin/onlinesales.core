// <copyright file="ElasticHelper.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel.DataAnnotations.Schema;
using Nest;

namespace OnlineSales.Elastic;

public class ElasticHelper
{
    public static void CreateMissingIndeces(ElasticDbContext dbContext)
    {
        var elasticClient = dbContext.ElasticClient;

        var migrationIndexName = GetIndexName(dbContext.IndexPrefix, typeof(ElasticMigration));

        if (elasticClient.Indices.Exists(migrationIndexName).Exists is false)
        {
            elasticClient.Indices.Create(migrationIndexName, f => f
                .Map(mapping => mapping
                    .AutoMap<ElasticMigration>()));
        }
    }

    public static async Task MigrateIndex(ElasticDbContext dbContext, Type entityType)
    {
        var elasticClient = dbContext.ElasticClient;
        var indexName = GetIndexName(dbContext.IndexPrefix, entityType);

        var oldMigrationIndexName = (await elasticClient.GetIndicesPointingToAliasAsync(indexName)).FirstOrDefault();

        if (string.IsNullOrEmpty(oldMigrationIndexName) && elasticClient.Indices.Exists(indexName).Exists)
        {
            oldMigrationIndexName = indexName;
        }

        var newMigrationIndexName = $"{indexName}-{Guid.NewGuid().ToString()}";

        if (oldMigrationIndexName == null)
        {
            await elasticClient.Indices.CreateAsync(newMigrationIndexName, f => f
                .Map(mapping => mapping
                    .AutoMap(entityType)));

            await elasticClient.Indices.PutAliasAsync(new PutAliasDescriptor(newMigrationIndexName, indexName));
        }
        else
        {
            elasticClient.Indices.Create(newMigrationIndexName, f => f
                .Map(mapping => mapping
                    .AutoMap(entityType)));

            var reindexResponse = await elasticClient.ReindexOnServerAsync(r =>
            {
                r = r.Source(s => s.Index(oldMigrationIndexName));
                r = r.Destination(s => s.Index(newMigrationIndexName));
                r = r.WaitForCompletion();

                return r;
            });

            if (reindexResponse.IsValid is false)
            {
                throw reindexResponse.OriginalException;
            }

            elasticClient.Indices.Delete(oldMigrationIndexName);            
            elasticClient.Indices.PutAlias(new PutAliasDescriptor(newMigrationIndexName, indexName));
        }
    }

    public static string GetIndexName(string indexPrefix, Type entityType)
    {
        var tableAttribute = entityType.GetCustomAttributes(typeof(TableAttribute), true).FirstOrDefault() as TableAttribute;

        if (tableAttribute != null)
        {
            return $"{indexPrefix}-{tableAttribute.Name}";
        }
        else
        {
            return $"{indexPrefix}-{nameof(entityType).ToLower()}";
        }
    }
}

