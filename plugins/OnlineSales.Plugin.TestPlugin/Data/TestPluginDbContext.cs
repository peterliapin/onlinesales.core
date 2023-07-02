// <copyright file="TestPluginDbContext.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Nest;
using Newtonsoft.Json.Linq;
using OnlineSales.Data;
using OnlineSales.Helpers;
using OnlineSales.Interfaces;
using OnlineSales.Plugin.TestPlugin.Entities;
using OnlineSales.Plugin.TestPlugin.TestData;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace OnlineSales.Plugin.TestPlugin.Data;

public class TestPluginDbContext : PluginDbContextBase
{
    public TestPluginDbContext()
        : base()
    {
    }

    public TestPluginDbContext(DbContextOptions<PgDbContext> options, IConfiguration configuration, IHttpContextHelper httpContextHelper)
        : base(options, configuration, httpContextHelper)
    {
    }

    public virtual DbSet<TestEntity>? TestEntities { get; set; }

#pragma warning disable S3400
    public string IsChangeLogMigrationsOk()
    {
        var migrator = Database.GetService<IMigrator>();
        if (migrator == null)
        {
            return "Cannot find IMigrator";
        }

        var migrations = Database.GetPendingMigrations();
        var initialMigration = migrations.FirstOrDefault(m => m.Contains("Initial"));
        if (initialMigration == null)
        {
            return "Cannot find Initial migration";
        }

        migrator!.Migrate(initialMigration);

        var insertDataMigration = migrations.FirstOrDefault(m => m.Contains("InsertData"));
        if (insertDataMigration == null)
        {
            return "Cannot find InsertData migration";
        }

        migrator!.Migrate(insertDataMigration);
        if (!CheckInsertData())
        {
            return "Insert data is incorrect";
        }

        var updateDataMigration = migrations.FirstOrDefault(m => m.Contains("UpdateData"));
        if (updateDataMigration == null)
        {
            return "Cannot find UpdateData migration";
        }

        migrator!.Migrate(updateDataMigration);
        if (!CheckUpdateData())
        {
            return "Update data is incorrect";
        }

        var deletedEntities = TestEntities!.Take(ChangeLogMigrationsTestData.NumberOfDeletedEntities).ToList();
        var deleteDataMigration = migrations.FirstOrDefault(m => m.Contains("DeleteData"));
        if (deleteDataMigration == null)
        {
            return "Cannot find DeleteData migration";
        }

        migrator!.Migrate(deleteDataMigration);
        if (!CheckDeleteData(deletedEntities))
        {
            return "Delete data is incorrect";
        }

        var addColumnMigration = migrations.FirstOrDefault(m => m.Contains("AddColumn"));
        if (addColumnMigration == null)
        {
            return "Cannot find AddColumn migration";
        }

        migrator!.Migrate(addColumnMigration);
        if (!CheckAddColumn())
        {
            return "Add column is incorrect";
        }

        var dropColumnMigration = migrations.FirstOrDefault(m => m.Contains("DropColumn"));
        if (dropColumnMigration == null)
        {
            return "Cannot find DropColumn migration";
        }

        migrator!.Migrate(dropColumnMigration);
        if (!CheckDropColumn())
        {
            return "Drop column is incorrect";
        }

        var dropTableMigration = migrations.FirstOrDefault(m => m.Contains("DropTable"));
        if (dropTableMigration == null)
        {
            return "Cannot find DropTable migration";
        }

        migrator!.Migrate(dropTableMigration);
        if (!CheckDropTable())
        {
            return "Drop table is incorrect";
        }

        return string.Empty;
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

    private bool CheckDropTable()
    {
        return !ChangeLogs!.Any(c => c.ObjectType == "TestEntity");
    }

    private bool CheckDropColumn()
    {
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

    private bool CheckAddColumn()
    {
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

    private bool CheckDeleteData(List<TestEntity> deletedEntities)
    {
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

    private bool CheckUpdateData()
    {
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

    private bool CheckInsertData()
    {
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