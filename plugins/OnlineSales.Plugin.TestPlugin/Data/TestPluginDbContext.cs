// <copyright file="TestPluginDbContext.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using OnlineSales.Data;
using OnlineSales.Helpers;
using OnlineSales.Interfaces;
using OnlineSales.Plugin.TestPlugin.Entities;
using OnlineSales.Plugin.TestPlugin.TestData;
using OnlineSales.Tests.Interfaces;

namespace OnlineSales.Plugin.TestPlugin.Data;

public class TestPluginDbContext : PluginDbContextBase, ITestMigrationService
{
    private readonly SortedSet<string> migrations;

    private readonly Dictionary<string, Func<TestPluginDbContext, IMigrator, string, bool>> checkers;

    public TestPluginDbContext()
        : base()
    {
        migrations = new SortedSet<string>();
        checkers = new Dictionary<string, Func<TestPluginDbContext, IMigrator, string, bool>>();
    }

    public TestPluginDbContext(DbContextOptions<PgDbContext> options, IConfiguration configuration, IHttpContextHelper httpContextHelper)
        : base(options, configuration, httpContextHelper)
    {
        migrations = new SortedSet<string>(Database.GetPendingMigrations());
        checkers = new Dictionary<string, Func<TestPluginDbContext, IMigrator, string, bool>>();
        checkers.Add("InsertData", (context, migrator, migration) => context.MigrateAndCheckInsertData(migrator, migration));
        checkers.Add("UpdateData", (context, migrator, migration) => context.MigrateAndCheckUpdateData(migrator, migration));
        checkers.Add("DeleteData", (context, migrator, migration) => context.MigrateAndCheckDeleteData(migrator, migration));
        checkers.Add("AddColumn", (context, migrator, migration) => context.MigrateAndCheckAddColumn(migrator, migration));
        checkers.Add("DropColumn", (context, migrator, migration) => context.MigrateAndCheckDropColumn(migrator, migration));
        checkers.Add("DropTable", (context, migrator, migration) => context.MigrateAndCheckDropTable(migrator, migration));
    }

    public virtual DbSet<TestEntity>? TestEntities { get; set; }

    public (bool, string) MigrateUpToAndCheck(string name)
    {
        var migrator = Database.GetService<IMigrator>();
        var migration = migrations.FirstOrDefault(m => m.Contains(name));
        if (migration == null)
        {
            return (false, $"Cannot find migration with name containig '{name}'");
        }

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

        Func<TestPluginDbContext, IMigrator, string, bool>? checker;

        if (!checkers.TryGetValue(name, out checker))
        {
            return (false, "Cannot find check function for migration with name containig '{name}'");
        }
        else
        {
            var res = checker(this, migrator, migration);
            if (!res)
            {
                return (false, $"{name} operation is failed");
            }
        }

        return (true, string.Empty);
    }
        
    private static string SnakeCaseToCamelCase(string input)
    {
        var words = input.Split('_');
        var result = words.First() + string.Concat(words.Skip(1).Select(w => char.ToUpperInvariant(w[0]) + w.Substring(1)));
        return result;
    }

    private bool GetIntProperty(JsonElement je, string key, out int value)
    {
        try
        {
            var prop = je.GetProperty(key);
            value = prop.GetInt32();
            return true;
        }
        catch
        {
            value = 0;
            return false;
        }
    }

    private void ReloadContextData()
    {
        var entries = ChangeTracker.Entries().ToList();
        foreach (var entry in entries)
        {
            entry.Reload();
        }
    }
        
    private bool MigrateAndCheckDropTable(IMigrator migrator, string migration)
    {
        migrator.Migrate(migration);
        return !ChangeLogs!.Any(c => c.ObjectType == "TestEntity");
    }

    private bool MigrateAndCheckDropColumn(IMigrator migrator, string migration)
    {
        migrator.Migrate(migration);
        ReloadContextData();

        var key = SnakeCaseToCamelCase(ChangeLogMigrationsTestData.AddedColumnName);

        foreach (var cl in ChangeLogs!)
        {
            var jsonData = JsonSerializer.Deserialize<JsonDocument>(cl.Data); 
            if (GetIntProperty(jsonData!.RootElement, key, out _))
            {
                return false;
            }
        }

        return true;
    }
    
    private bool MigrateAndCheckAddColumn(IMigrator migrator, string migration)
    {
        migrator.Migrate(migration);
        ReloadContextData();

        var key = SnakeCaseToCamelCase(ChangeLogMigrationsTestData.AddedColumnName);

        foreach (var cl in ChangeLogs!)
        {
            var jsonData = JsonSerializer.Deserialize<JsonDocument>(cl.Data);
            int value;
            if (!GetIntProperty(jsonData!.RootElement, key, out value) || value != ChangeLogMigrationsTestData.AddedColumnDefaultValue)
            {
                return false;
            }           
        }

        return true;
    }

    private bool MigrateAndCheckDeleteData(IMigrator migrator, string migration)
    {
        var deletedEntities = TestEntities!.Take(ChangeLogMigrationsTestData.NumberOfDeletedEntities).ToList();
        migrator.Migrate(migration);
        ReloadContextData();

        var te = TestEntities!.ToList();
        if (te.Count != ChangeLogMigrationsTestData.NumberOfRecords - ChangeLogMigrationsTestData.NumberOfDeletedEntities)
        {
            return false;
        }

        var cl = ChangeLogs!.OrderByDescending(c => c.Id).Take(ChangeLogMigrationsTestData.NumberOfDeletedEntities).OrderBy(c => c.Id).ToList();

        for (int i = 0; i < cl.Count; ++i)
        {
            var jsonData = JsonHelper.Deserialize<TestEntity>(cl[i].Data);
            if (jsonData!.StringField != deletedEntities[i].StringField || cl[i].EntityState != EntityState.Deleted || cl[i].ObjectType != "TestEntity" || cl[i].ObjectId != deletedEntities[i].Id)
            {
                return false;
            }
        }

        return true;
    }

    private bool MigrateAndCheckUpdateData(IMigrator migrator, string migration)
    {
        migrator.Migrate(migration);
        ReloadContextData();

        var te = TestEntities!.ToList();
        var cl = ChangeLogs!.Skip(ChangeLogMigrationsTestData.InsertionData.Length).Take(ChangeLogMigrationsTestData.InsertionData.Length).ToList();

        if (te.Count != cl.Count)
        {
            return false;
        }

        for (int i = 0; i < te.Count; ++i)
        {
            var jsonData = JsonHelper.Deserialize<TestEntity>(cl[i].Data);
            if (jsonData!.StringField != te[i].StringField || cl[i].EntityState != EntityState.Modified || cl[i].ObjectType != "TestEntity" || cl[i].ObjectId != te[i].Id)
            {
                return false;
            }
        }

        return true;
    }

    private bool MigrateAndCheckInsertData(IMigrator migrator, string migration)
    {
        migrator.Migrate(migration);
        ReloadContextData();

        var te = TestEntities!.ToList();
        var cl = ChangeLogs!.ToList();

        if (te.Count != cl.Count)
        {
            return false;
        }

        for (int i = 0; i < TestEntities!.Count(); ++i)
        {
            var jsonData = JsonHelper.Deserialize<TestEntity>(cl[i].Data);
            if (jsonData!.StringField != te[i].StringField || cl[i].EntityState != EntityState.Added || cl[i].ObjectType != "TestEntity" || cl[i].ObjectId != te[i].Id)
            {
                return false;
            }
        }

        return true;
    }
}