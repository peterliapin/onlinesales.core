// <copyright file="CommentsTests.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Net;

namespace OnlineSales.Tests;

public class CommentsTests : BaseTest
{
    [Fact]
    public async Task GetCommentsTest()
    {
        await GetTest("/api/comments", HttpStatusCode.OK);
    }
}