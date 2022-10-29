// <copyright file="BlogPostTests.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Net;
using OnlineSales.Tests.TestEntities;

namespace OnlineSales.Tests;

public class PostTests : BaseTest
{
    [Fact]
    public async Task CreatePostTest()
    {
        var post = new TestPost();

        await PostTest("/api/posts", post, HttpStatusCode.Created);
    }
}

