// <copyright file="BlogPostTests.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using FluentAssertions;
using OnlineSales.Entities;

namespace OnlineSales.Tests;

public class PostTests : BaseTest
{
    [Fact]
    public async Task GetPostNotFoundTest()
    {
        await GetTest("/api/posts/404", HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateAndGetPostTest()
    {
        var testPost = new TestPostCreateDto();

        var newPostUrl = await PostTest("/api/posts", testPost);

        var post = await GetTest<Post>(newPostUrl);

        post.Should().BeEquivalentTo(testPost);
    }

    [Fact]
    public async Task UpdatePostNotFoundTest()
    {
        await PatchTest("/api/posts/404", new { }, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateAndUpdatePostTitleTest()
    {
        var testPost = new TestPostCreateDto();

        var newPostUrl = await PostTest("/api/posts", testPost);

        var updatedPost = new TestPostUpdateDto();

        testPost.Title = updatedPost.Title = "Test Updated Title (via test suit)";

        await PatchTest(newPostUrl, updatedPost);

        var post = await GetTest<Post>(newPostUrl);

        post.Should().BeEquivalentTo(testPost);
    }

    [Fact]
    public async Task DeletePostNotFoundTest()
    {
        await DeleteTest("/api/posts/404", HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateAndDeletePostTest()
    {
        var testPost = new TestPostCreateDto();

        var newPostUrl = await PostTest("/api/posts", testPost);

        await DeleteTest(newPostUrl);

        await GetTest(newPostUrl, HttpStatusCode.NotFound);
    }
}

