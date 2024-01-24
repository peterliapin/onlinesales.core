// <copyright file="TestPluginDbContext.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using OnlineSales.Data;
using OnlineSales.Interfaces;
using OnlineSales.Plugin.TestPlugin.Entities;
using OnlineSales.Plugin.TestPlugin.Exceptions;

namespace OnlineSales.Plugin.TestPlugin.Data;

public class TestPluginDbContext : PluginDbContextBase
{
    private readonly SortedSet<string> migrations;

    public TestPluginDbContext()
        : base()
    {
        migrations = new SortedSet<string>();
    }

    public TestPluginDbContext(DbContextOptions<PgDbContext> options, IConfiguration configuration, IHttpContextHelper httpContextHelper)
        : base(options, configuration, httpContextHelper)
    {
        migrations = new SortedSet<string>(Database.GetPendingMigrations());
    }

    public virtual DbSet<TestEntity>? TestEntities { get; set; }

    public void MigrateUntil(string name)
    {
        var migrator = Database.GetService<IMigrator>();
        var migration = FindMigration(name);
        foreach (var m in migrations)
        {
            if (m != migration)
            {
                migrator.Migrate(m);
            }
            else
            {
                break;
            }
        }
    }

    public void MigrateUpTo(string name)
    {
        MigrateUntil(name);
        Migrate(name);
    }

    public void Migrate(string name)
    {
        var migrator = Database.GetService<IMigrator>();
        var migration = FindMigration(name);
        migrator.Migrate(migration);
    }

    private string FindMigration(string name)
    {
        var migration = migrations.FirstOrDefault(m => m.Contains(name));
        if (migration == null)
        {
            throw new TestDbContextException($"Cannot find migration with name containig '{name}'");
        }

        return migration;    
    }
}