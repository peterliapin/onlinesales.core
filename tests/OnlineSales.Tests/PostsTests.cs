// <copyright file="PostsTests.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using FluentAssertions;
using OnlineSales.DTOs;
using OnlineSales.Entities;
using OnlineSales.Tests.TestEntities.BulkPopulate;

namespace OnlineSales.Tests;

public class PostsTests : SimpleTableTests<Post, TestPost, PostUpdateDto, PostDetailsDto, TestBulkPosts>
{
    public PostsTests()
        : base("/api/posts")
    {
    }

    [Fact]
    public async Task GetAllTestAnonymous()
    {
        await GetAllWithAuthentification("NonSuccess");
    }

    [Fact]
    public async Task CreateAndGetItemTestAnonymous()
    {
        await CreateAndGetItemWithAuthentification("NonSuccess");
    }

    protected override PostUpdateDto UpdateItem(TestPost to)
    {
        var from = new PostUpdateDto();
        to.Template = from.Template = to.Template + "Updated";
        return from;
    }
}