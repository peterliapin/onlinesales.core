// <copyright file="ChangeLogMigrationsTests.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using Microsoft.Extensions.DependencyInjection;
using OnlineSales.Tests.Interfaces;
using Xunit.Abstractions;

namespace OnlineSales.Tests;
public class ChangeLogMigrationsTests : BaseTest
{
    private readonly ITestOutputHelper outputHelper;

    public ChangeLogMigrationsTests(ITestOutputHelper outputHelper)
    {
        this.outputHelper = outputHelper;
    }

    [Theory]
    [InlineData("InsertData")]
    [InlineData("DeleteData")]
    [InlineData("UpdateData")]
    [InlineData("AddColumn")]
    [InlineData("DropColumn")]
    [InlineData("DropTable")]
    public void ChangeLogMigrationTest(string name)
    {
        using (var scope = App.Services.CreateScope())
        {
            var migrationService = scope.ServiceProvider.GetService<ITestMigrationService>();
            migrationService.Should().NotBeNull();
            var res = migrationService!.MigrateUpToAndCheck(name);
            if (!res.Item1)
            {
                outputHelper.WriteLine(res.Item2);
            }

            res.Item1.Should().BeTrue();
        }
    }
}