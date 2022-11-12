// <copyright file="LogsTests.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using FluentAssertions;
using OnlineSales.Entities;

namespace OnlineSales.Tests;

public class LogsTests : BaseTest
{
    [Fact]
    public async Task GetLogsTest()
    {
        var logRecord = await GetTest<List<LogRecord>>("/api/logs", HttpStatusCode.OK);

        logRecord.Should().NotBeEmpty();
    }
}

