// <copyright file="LogTests.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

namespace OnlineSales.Tests;

public class LogTests : BaseTest
{
    /// <summary>
    /// Simply verifies that logs API can be successfully executed and returns HTTP 200.
    /// We can not really verify log records as they are added to Elastic asynchromously.
    /// </summary>
    [Fact]
    public async Task GetLogsTest()
    {
        await GetTest("/api/logs");
    }
}