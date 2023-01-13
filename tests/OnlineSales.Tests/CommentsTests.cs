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
        comment.Content = "Content";
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

        updatedComment!.PostId.Should().Be(1);
        updatedComment!.AuthorName.Should().Be("TestComment1");

        var newComment = await GetTest<Comment>($"{itemsUrl}/2");
        newComment.Should().NotBeNull();

        newComment!.PostId.Should().Be(1);
        newComment!.AuthorName.Should().Be("TestComment2");
        newComment!.CreatedAt.Should().Be(DateTime.Parse("2023-01-04T08:38:30"));
    }

    [Theory]
    [InlineData("commentsFull.csv")]
    [InlineData("commentsFull.json")]
    public async Task ImportFileAddUpdateAllFieldsTest(string fileName)
    {
        await CreateAndGetItemWithAuthentification("Anonymous");
        await PostImportTest(itemsUrl, fileName);

        var updatedComment = await GetTest<Comment>($"{itemsUrl}/1");
        updatedComment.Should().NotBeNull();

        updatedComment!.PostId.Should().Be(1);
        updatedComment!.AuthorName.Should().Be("TestComment1");
        updatedComment!.UpdatedAt.Should().Be(DateTime.Parse("01/06/2023 09:36:16"));
        /*updatedComment!.CreatedAt.Should().BeAfter(DateTime.MinValue);
        updatedComment!.CreatedByIp.Should().Be("192.168.1.1");
        updatedComment!.UpdatedByIp.Should().Be("192.168.1.3");
        updatedComment!.CreatedByUserAgent.Should().Be("TestAgent1");
        updatedComment!.UpdatedByUserAgent.Should().Be("TestAgent3");*/

        var newComment = await GetTest<Comment>($"{itemsUrl}/2");
        newComment.Should().NotBeNull();

        newComment!.PostId.Should().Be(1);
        newComment!.AuthorName.Should().Be("TestComment2");
        newComment!.CreatedAt.Should().Be(DateTime.Parse("2023-01-04T08:38:30"));
        newComment!.UpdatedAt.Should().BeNull();
        /*newComment!.CreatedByIp.Should().Be("192.168.1.2");
        newComment!.CreatedByUserAgent.Should().Be("TestAgent2");*/
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