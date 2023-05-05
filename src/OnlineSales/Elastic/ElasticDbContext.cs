// <copyright file="ElasticDbContext.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Reflection;
using Nest;

namespace OnlineSales.Elastic;

public abstract class ElasticDbContext
{
    public abstract ElasticClient ElasticClient { get; }

    public abstract string IndexPrefix { get; }

    protected abstract List<Type> EntityTypes { get; }

    public virtual void Migrate()
    {
        ElasticHelper.CreateMissingIndeces(this);

        var allMigrationsTypes = Assembly.GetAssembly(typeof(ElasticMigration))!.GetTypes()
                                    .Where(
                                        myType => myType.IsClass
                                        && !myType.IsAbstract
                                        && myType.IsSubclassOf(typeof(ElasticMigration)))
                                    .OrderBy(type => type.Name);

        var pastMigrationIds = this.ElasticClient.Search<ElasticMigration>(s => s.Size(10000)).Documents // 10000 is max possible amount of migrations
                                .Select(m => m.MigrationId).ToList();

        foreach (var type in allMigrationsTypes)
        {
            var migration = (ElasticMigration)Activator.CreateInstance(type)!;

            if (pastMigrationIds.Contains(migration.MigrationId) is false)
            {
                migration.Up(this).Wait();
                this.ElasticClient.Index<ElasticMigration>(migration, s => s);
            }
        }
    }
}
