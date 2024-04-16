// <copyright file="ChangeLogMigrationsTests.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OnlineSales.Helpers;
using OnlineSales.Plugin.TestPlugin.Data;
using OnlineSales.Plugin.TestPlugin.Entities;
using OnlineSales.Plugin.TestPlugin.TestData;

namespace OnlineSales.Tests;

public class ChangeLogMigrationsTests : BaseTest
{
    [Fact]
    public void InsertDataTest()
    {
        var name = "InsertData";

        using (var scope = App.Services.CreateScope())
        {
            var dbContext = GetDbContext(scope);
            dbContext!.MigrateUpTo(name);

            var te = dbContext.TestEntities!.ToList();
            var cl = dbContext.ChangeLogs!.ToList();

            te.Count.Should().Be(cl.Count);

            for (int i = 0; i < te.Count; ++i)
            {
                var jsonData = JsonHelper.Deserialize<TestEntity>(cl[i].Data);
                jsonData!.StringField.Should().Be(te[i].StringField);
                cl[i].EntityState.Should().Be(EntityState.Added);
                cl[i].ObjectType.Should().Be("TestEntity");
                cl[i].ObjectId.Should().Be(te[i].Id);
            }
        }
    }

    [Fact]

    public void DeleteDataTest()
    {
        var name = "DeleteData";

        using (var scope = App.Services.CreateScope())
        {
            var dbContext = GetDbContext(scope);
            dbContext!.MigrateUntil(name);

            var deletedEntities = dbContext.TestEntities!.Take(ChangeLogMigrationsTestData.NumberOfDeletedEntities).ToList();

            dbContext.Migrate(name);

            var te = dbContext.TestEntities!.ToList();
            te.Count.Should().Be(ChangeLogMigrationsTestData.NumberOfRecords - ChangeLogMigrationsTestData.NumberOfDeletedEntities);

            var cl = dbContext.ChangeLogs!.OrderByDescending(c => c.Id).Take(ChangeLogMigrationsTestData.NumberOfDeletedEntities).OrderBy(c => c.Id).ToList();

            for (int i = 0; i < cl.Count; ++i)
            {
                var jsonData = JsonHelper.Deserialize<TestEntity>(cl[i].Data);
                jsonData!.StringField.Should().Be(deletedEntities[i].StringField);
                cl[i].EntityState.Should().Be(EntityState.Deleted);
                cl[i].ObjectType.Should().Be("TestEntity");
                cl[i].ObjectId.Should().Be(deletedEntities[i].Id);
            }
        }
    }

    [Fact]

    public void UpdateDataTest()
    {
        var name = "UpdateData";

        using (var scope = App.Services.CreateScope())
        {
            var dbContext = GetDbContext(scope);
            dbContext!.MigrateUpTo(name);

            var te = dbContext.TestEntities!.ToList();
            var cl = dbContext.ChangeLogs!.Skip(ChangeLogMigrationsTestData.InsertionData.Count).Take(ChangeLogMigrationsTestData.InsertionData.Count).ToList();

            te.Count.Should().Be(cl.Count);

            for (int i = 0; i < cl.Count; ++i)
            {
                var jsonData = JsonHelper.Deserialize<TestEntity>(cl[i].Data);
                jsonData!.StringField.Should().Be(te[i].StringField);
                cl[i].EntityState.Should().Be(EntityState.Modified);
                cl[i].ObjectType.Should().Be("TestEntity");
                cl[i].ObjectId.Should().Be(te[i].Id);
            }
        }
    }

    [Fact]
    public void AddColumnTest()
    {
        var name = "AddColumn";

        using (var scope = App.Services.CreateScope())
        {
            var dbContext = GetDbContext(scope);
            dbContext!.MigrateUpTo(name);

            var key = SnakeCaseToCamelCase(ChangeLogMigrationsTestData.AddedColumnName);
            var changeLogs = dbContext.ChangeLogs!;

            foreach (var cl in changeLogs)
            {
                var jsonData = JsonSerializer.Deserialize<JsonDocument>(cl.Data);
                var res = GetIntProperty(jsonData!.RootElement, key, out var value);
                res.Should().Be(true);
                value.Should().Be(ChangeLogMigrationsTestData.AddedColumnDefaultValue);
            }
        }
    }

    [Fact]
    public void DropColumnTest()
    {
        var name = "DropColumn";

        using (var scope = App.Services.CreateScope())
        {
            var dbContext = GetDbContext(scope);
            dbContext!.MigrateUpTo(name);

            var key = SnakeCaseToCamelCase(ChangeLogMigrationsTestData.AddedColumnName);
            var changeLogs = dbContext.ChangeLogs!;

            foreach (var cl in changeLogs)
            {
                var jsonData = JsonSerializer.Deserialize<JsonDocument>(cl.Data);
                var res = GetIntProperty(jsonData!.RootElement, key, out _);
                res.Should().Be(false);
            }
        }
    }

    [Fact]
    public void DropTableTest()
    {
        var name = "DropTable";

        using (var scope = App.Services.CreateScope())
        {
            var dbContext = GetDbContext(scope);
            dbContext!.MigrateUpTo(name);
            dbContext!.ChangeLogs!.Any(c => c.ObjectType == "TestEntity").Should().Be(false);
        }
    }

    private static string SnakeCaseToCamelCase(string input)
    {
        var words = input.Split('_');
        var result = words.First() + string.Concat(words.Skip(1).Select(w => char.ToUpperInvariant(w[0]) + w.Substring(1)));
        return result;
    }

    private static bool GetIntProperty(JsonElement je, string key, out int value)
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

    private TestPluginDbContext GetDbContext(IServiceScope scope)
    {
        var res = scope.ServiceProvider.GetService<TestPluginDbContext>();
        res.Should().NotBeNull();
        return res!;
    }
}