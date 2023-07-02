// <copyright file="ChangeLogMigrationsTests.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using OnlineSales.Data;

namespace OnlineSales.Tests;
public class ChangeLogMigrationsTests : BaseTest
{
    [Fact]
    public void InsertDataTest()
    {
        using (var scope = App.Services.CreateScope())
        {
            var pc = scope.ServiceProvider.GetServices<PluginDbContextBase>();
            pc.Count().Should().Be(1);
            var ts = pc.First();
            ts.Should().NotBeNull();
            var t = ts.GetType();
            t.Should().NotBeNull();
            var tp = t.GetMethod("IsChangeLogMigrationsOk");
            tp.Should().NotBeNull();
            var res = (string)tp!.Invoke(ts, new object?[0])!;
            if (res.Length > 0)
            {
                Assert.True(false, res);
            }
        }
    }
}