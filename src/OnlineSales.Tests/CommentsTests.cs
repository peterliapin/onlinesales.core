// <copyright file="CommentsTests.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using Microsoft.AspNetCore.Mvc.Testing;

namespace OnlineSales.Tests;

public class CommentsTests : BaseTest
{
    [Theory]
    [InlineData("/api/comments")]
    [InlineData("/api/posts")]
    public async Task Test1(string url)
    {
        var response = await client.GetAsync(url);

        response.EnsureSuccessStatusCode();

        Assert.Equal("text/html; charset=utf-8", response?.Content?.Headers?.ContentType?.ToString());
    }
}
