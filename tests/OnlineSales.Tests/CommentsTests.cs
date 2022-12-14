// <copyright file="CommentsTests.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using FluentAssertions;
using OnlineSales.DTOs;
using OnlineSales.Entities;

namespace OnlineSales.Tests;

public class CommentsTests : TableWithFKTests<Comment, TestComment, CommentUpdateDto>
{
    public CommentsTests()
        : base("/api/comments")
    {
    }

    [Fact]
    public async Task GetAllTestAnonymous()
    {
        await GetAllWithAuthentification("Anonymous");
    }

    [Fact]
    public async Task CreateAndGetItemTestAnonymous()
    {
        await CreateAndGetItemWithAuthentification("Anonymous");
    }

    [Fact]
    public override async Task UpdateItemNotFoundTest()
    {
        var comment = new CommentUpdateDto();
        comment.Content = "Content";
        await PatchTest(itemsUrlNotFound, comment, HttpStatusCode.NotFound);
    }

    protected override async Task<(TestComment, string)> CreateItem(int fkId)
    {
        var testComment = new TestComment(string.Empty, fkId);

        var newCommentUrl = await PostTest(itemsUrl, testComment);

        return (testComment, newCommentUrl);
    }

    protected override async Task<(int, string)> CreateFKItem()
    {
        var fkItemCreate = new TestPost();

        var fkUrl = await PostTest("/api/posts", fkItemCreate);

        var fkItem = await GetTest<Post>(fkUrl);

        fkItem.Should().NotBeNull();

        return (fkItem!.Id, fkUrl);
    }

    protected override CommentUpdateDto UpdateItem(TestComment to)
    {
        var from = new CommentUpdateDto();
        to.Content = from.Content = to.Content + "Updated";
        return from;
    }
}