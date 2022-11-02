// <copyright file="BlogPostTests.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using FluentAssertions;
using OnlineSales.Entities;

namespace OnlineSales.Tests;

public class PostTests : BaseTest
{
    [Fact]
    public async Task CreateAndGetPostTest()
    {
        var testPost = new TestPostCreateDto();

        var newPostUrl = await PostTest("/api/posts", testPost);

        var post = await GetTest<Post>(newPostUrl);

        post.Should().BeEquivalentTo(testPost);
    }

    [Fact]
    public async Task UpdatePostTitleTest()
    {
        var testPost = new TestPostCreateDto();

        var newPostUrl = await PostTest("/api/posts", testPost);

        var updatedPost = new TestPostUpdateDto();

        testPost.Title = updatedPost.Title = "Test Updated Title (via test suit)";

        await PutTest(newPostUrl, updatedPost);

        var post = await GetTest<Post>(newPostUrl);

        post.Should().BeEquivalentTo(testPost);
    }
}

