// <copyright file="CommentsTests.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Text.Json;
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
        comment.Body = "Content";
        await PatchTest(itemsUrlNotFound, comment, HttpStatusCode.NotFound);
    }

    [Theory]
    [InlineData("commentsBasic.csv", 2)]
    [InlineData("commentsBasic.json", 2)]
    public async Task ImportFileAddUpdateBasicTest(string fileName, int expectedCount)
    {
        await CreateAndGetItemWithAuthentification("Anonymous");
        await PostImportTest(itemsUrl, fileName);

        var newComment = await GetTest<Comment>($"{itemsUrl}/2");
        newComment.Should().NotBeNull();

        await SyncElasticSearch(itemsUrl, 2);

        var allCommentsResponse = await GetTest(itemsUrl);
        allCommentsResponse.Should().NotBeNull();

        var content = await allCommentsResponse.Content.ReadAsStringAsync();
        var allComments = JsonSerializer.Deserialize<List<Comment>>(content);
        allComments.Should().NotBeNull();
        allComments!.Count.Should().Be(expectedCount);
    }

    [Theory]
    [InlineData("commentsBasic.csv")]
    [InlineData("commentsBasic.json")]
    public async Task ImportFileAddUpdateDataTest(string fileName)
    {
        await CreateAndGetItemWithAuthentification("Anonymous");
        await PostImportTest(itemsUrl, fileName);

        var updatedComment = await GetTest<Comment>($"{itemsUrl}/1");
        updatedComment.Should().NotBeNull();

        updatedComment!.ContentId.Should().Be(1);
        updatedComment!.AuthorName.Should().Be("TestComment1");

        var newComment = await GetTest<Comment>($"{itemsUrl}/2");
        newComment.Should().NotBeNull();

        newComment!.ContentId.Should().Be(1);
        newComment!.AuthorName.Should().Be("TestComment2");
        newComment!.CreatedAt.Should().Be(DateTime.Parse("2023-01-15T17:32:02.074179Z").ToUniversalTime());
    }

    [Theory]
    [InlineData("commentsFull.csv")]
    [InlineData("commentsFull.json")]
    public async Task ImportFileAddUpdateAllFieldsTest(string fileName)
    {
        await CreateAndGetItemWithAuthentification("Anonymous");
        await PostImportTest(itemsUrl, fileName);

        var updatedComment = App.GetDbContext() !.Comments!.First(c => c.Id == 1);
        updatedComment.Should().NotBeNull();

        updatedComment!.ContentId.Should().Be(1);
        updatedComment!.AuthorName.Should().Be("TestComment1");
        updatedComment!.UpdatedAt.Should().Be(DateTime.Parse("2023-01-15T17:32:02.074179Z").ToUniversalTime());
        updatedComment!.CreatedAt.Should().Be(DateTime.Parse("2023-01-15T17:32:02.074179Z").ToUniversalTime());
        updatedComment!.CreatedByIp.Should().Be("192.168.1.1");
        updatedComment!.UpdatedByIp.Should().Be("192.168.1.3");
        updatedComment!.CreatedByUserAgent.Should().Be("TestAgent1");
        updatedComment!.UpdatedByUserAgent.Should().Be("TestAgent3");

        var newComment = App.GetDbContext() !.Comments!.First(c => c.Id == 2);
        newComment.Should().NotBeNull();

        newComment!.ContentId.Should().Be(1);
        newComment!.AuthorName.Should().Be("TestComment2");
        newComment!.CreatedAt.Should().Be(DateTime.Parse("2023-01-15T17:32:02.074179Z").ToUniversalTime());
        newComment!.UpdatedAt.Should().BeNull();
        newComment!.CreatedByIp.Should().Be("192.168.1.2");
        newComment!.CreatedByUserAgent.Should().Be("TestAgent2");
    }

    [Fact]
    public async Task ImportFileWithParentUniqueKeyTest()
    {
        await CreateFKItemsWithUid();
        await PostImportTest(itemsUrl, "commentsNoFKHasUKey.csv");

        var addedComment1 = App.GetDbContext() !.Comments!.First(c => c.Id == 1);
        addedComment1.Should().NotBeNull();
        addedComment1.ContentId.Should().Be(1);

        var addedComment2 = App.GetDbContext() !.Comments!.First(c => c.Id == 2);
        addedComment2.Should().NotBeNull();
        addedComment2.ContentId.Should().Be(2);

        await PostImportTest(itemsUrl, "commentsNoFKHasUKeyUpdate.csv");
        var updatedComment = App.GetDbContext() !.Comments!.First(c => c.Id == 1);
        updatedComment.Should().NotBeNull();
        updatedComment.ContentId.Should().Be(2);
    }

    [Fact]
    public async Task ImportFileWithParentKeysTest()
    {
        await CreateFKItemsWithUid();
        await PostImportTest(itemsUrl, "commentsWithParentKey.csv");

        var addedComment1 = App.GetDbContext() !.Comments!.First(c => c.Id == 1);
        addedComment1.Should().NotBeNull();

        var addedComment2 = App.GetDbContext() !.Comments!.First(c => c.Id == 2);
        addedComment2.Should().NotBeNull();
        addedComment2.ParentId.Should().Be(1);

        var addedComment3 = App.GetDbContext() !.Comments!.First(c => c.Id == 3);
        addedComment3.Should().NotBeNull();
        addedComment3.ParentId.Should().Be(1);

        await PostImportTest(itemsUrl, "commentsWithOldParentKey.csv");

        var addedComment4 = App.GetDbContext() !.Comments!.First(c => c.Id == 4);
        addedComment4.Should().NotBeNull();
        addedComment4.ParentId.Should().Be(1);

        var addedComment5 = App.GetDbContext() !.Comments!.First(c => c.Id == 5);
        addedComment5.Should().NotBeNull();
        addedComment5.ParentId.Should().Be(2);

        var addedComment6 = App.GetDbContext() !.Comments!.First(c => c.Id == 6);
        addedComment6.Should().NotBeNull();
        addedComment6.ParentId.Should().Be(4);
    }

    protected override async Task<(TestComment, string)> CreateItem(string uid, int fkId)
    {
        var testComment = new TestComment(uid, fkId);

        var newCommentUrl = await PostTest(itemsUrl, testComment);

        return (testComment, newCommentUrl);
    }

    protected override async Task<(int, string)> CreateFKItem()
    {
        var fkItemCreate = new TestContent();

        var fkUrl = await PostTest("/api/contents", fkItemCreate);

        var fkItem = await GetTest<Content>(fkUrl);

        fkItem.Should().NotBeNull();

        return (fkItem!.Id, fkUrl);
    }

    protected override CommentUpdateDto UpdateItem(TestComment to)
    {
        var from = new CommentUpdateDto();
        to.Body = from.Body = to.Body + "Updated";
        return from;
    }

    private async Task CreateFKItemsWithUid()
    {
        var fkItemCreate1 = new TestContent("100");
        var fkItemCreate2 = new TestContent("101");

        await PostTest("/api/contents", fkItemCreate1);
        await PostTest("/api/contents", fkItemCreate2);
    }
}