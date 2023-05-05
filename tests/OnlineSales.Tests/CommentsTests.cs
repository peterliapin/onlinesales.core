// <copyright file="CommentsTests.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Text.Json;

namespace OnlineSales.Tests;

public class CommentsTests : TableWithFKTests<Comment, TestComment, CommentUpdateDto, ICommentService>
{
    public CommentsTests()
        : base("/api/comments")
    {
    }

    [Fact]
    public async Task GetAllTestAnonymous()
    {
        await this.GetAllWithAuthentification("Anonymous");
    }

    [Fact]
    public async Task CreateAndGetItemTestAnonymous()
    {
        await this.CreateAndGetItemWithAuthentification("Anonymous");
    }

    [Fact]
    public override async Task UpdateItemNotFoundTest()
    {
        var comment = new CommentUpdateDto();
        comment.Body = "Content";
        await this.PatchTest(this.itemsUrlNotFound, comment, HttpStatusCode.NotFound);
    }

    [Theory]
    [InlineData("commentsBasic.csv", 2)]
    [InlineData("commentsBasic.json", 2)]
    public async Task ImportFileAddUpdateBasicTest(string fileName, int expectedCount)
    {
        await this.CreateFKItemsWithUid();
        await this.CreateItem();

        await this.PostImportTest(this.itemsUrl, fileName);

        var newComment = await this.GetTest<Comment>($"{this.itemsUrl}/2");
        newComment.Should().NotBeNull();

        var allCommentsResponse = await this.GetTest(this.itemsUrl);
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
        await this.CreateFKItemsWithUid();
        await this.CreateItem();

        await this.PostImportTest(this.itemsUrl, fileName);

        var updatedComment = await this.GetTest<Comment>($"{this.itemsUrl}/1");
        updatedComment.Should().NotBeNull();

        updatedComment!.ContentId.Should().Be(1);
        updatedComment!.ContactId.Should().NotBe(0);
        updatedComment!.AuthorName.Should().Be("Author Name 1");

        var newComment = await this.GetTest<Comment>($"{this.itemsUrl}/2");
        newComment.Should().NotBeNull();

        newComment!.ContentId.Should().Be(1);
        newComment!.ContactId.Should().NotBe(0);
        newComment!.AuthorName.Should().Be("Author Name 2");
        newComment!.CreatedAt.Should().Be(DateTime.Parse("2023-01-15T17:32:02.074179Z").ToUniversalTime());
    }

    [Theory]
    [InlineData("commentsFull.csv")]
    [InlineData("commentsFull.json")]
    public async Task ImportFileAddUpdateAllFieldsTest(string fileName)
    {
        await this.CreateFKItemsWithUid();
        await this.CreateItem();

        await this.PostImportTest(this.itemsUrl, fileName);

        var updatedComment = App.GetDbContext()!.Comments!.First(c => c.Id == 1);
        updatedComment.Should().NotBeNull();

        updatedComment!.ContentId.Should().Be(1);
        updatedComment!.ContactId.Should().NotBe(0);
        updatedComment!.AuthorName.Should().Be("Author Name 1");
        updatedComment!.UpdatedAt.Should().Be(DateTime.Parse("2023-01-15T17:32:02.074179Z").ToUniversalTime());
        updatedComment!.CreatedAt.Should().Be(DateTime.Parse("2023-01-15T17:32:02.074179Z").ToUniversalTime());
        updatedComment!.CreatedByIp.Should().Be("192.168.1.1");
        updatedComment!.UpdatedByIp.Should().Be("192.168.1.3");
        updatedComment!.CreatedByUserAgent.Should().Be("TestAgent1");
        updatedComment!.UpdatedByUserAgent.Should().Be("TestAgent3");

        var newComment = App.GetDbContext()!.Comments!.First(c => c.Id == 2);
        newComment.Should().NotBeNull();

        newComment!.ContentId.Should().Be(1);
        newComment!.ContactId.Should().NotBe(0);
        newComment!.AuthorName.Should().Be("Author Name 2");
        newComment!.CreatedAt.Should().Be(DateTime.Parse("2023-01-15T17:32:02.074179Z").ToUniversalTime());
        newComment!.UpdatedAt.Should().BeNull();
        newComment!.CreatedByIp.Should().Be("192.168.1.2");
        newComment!.CreatedByUserAgent.Should().Be("TestAgent2");
    }

    [Fact]
    public async Task ImportFileWithParentUniqueKeyTest()
    {
        await this.CreateFKItemsWithUid();
        await this.PostImportTest(this.itemsUrl, "commentsNoFKHasUKey.csv");

        var addedComment1 = App.GetDbContext()!.Comments!.First(c => c.Id == 1);
        addedComment1.Should().NotBeNull();
        addedComment1.ContentId.Should().Be(1);

        var addedComment2 = App.GetDbContext()!.Comments!.First(c => c.Id == 2);
        addedComment2.Should().NotBeNull();
        addedComment2.ContentId.Should().Be(2);

        await this.PostImportTest(this.itemsUrl, "commentsNoFKHasUKeyUpdate.csv");
        var updatedComment = App.GetDbContext()!.Comments!.First(c => c.Id == 1);
        updatedComment.Should().NotBeNull();
        updatedComment.ContentId.Should().Be(2);
    }

    [Fact]
    public async Task ImportFileWithParentKeysTest()
    {
        await this.CreateFKItemsWithUid(6);
        await this.PostImportTest(this.itemsUrl, "commentsWithParentKey.csv");

        var addedComment1 = App.GetDbContext()!.Comments!.First(c => c.Id == 1);
        addedComment1.Should().NotBeNull();

        var addedComment2 = App.GetDbContext()!.Comments!.First(c => c.Id == 2);
        addedComment2.Should().NotBeNull();
        addedComment2.ParentId.Should().Be(1);

        var addedComment3 = App.GetDbContext()!.Comments!.First(c => c.Id == 3);
        addedComment3.Should().NotBeNull();
        addedComment3.ParentId.Should().Be(1);

        await this.PostImportTest(this.itemsUrl, "commentsWithOldParentKey.csv");

        var addedComment4 = App.GetDbContext()!.Comments!.First(c => c.Id == 4);
        addedComment4.Should().NotBeNull();
        addedComment4.ParentId.Should().Be(1);

        var addedComment5 = App.GetDbContext()!.Comments!.First(c => c.Id == 5);
        addedComment5.Should().NotBeNull();
        addedComment5.ParentId.Should().Be(2);

        var addedComment6 = App.GetDbContext()!.Comments!.First(c => c.Id == 6);
        addedComment6.Should().NotBeNull();
        addedComment6.ParentId.Should().Be(4);
    }

    [Fact]
    public async Task MultiIterationsImportTest()
    {
        await this.CreateFKItemsWithUid(1);

        var importResult = await this.PostImportTest(this.itemsUrl, "commentsWithKey.csv");

        importResult.Added.Should().Be(4);
        importResult.Updated.Should().Be(0);
        importResult.Failed.Should().Be(0);
        importResult.Skipped.Should().Be(0);

        importResult = await this.PostImportTest(this.itemsUrl, "commentsWithKeyUpdate.csv");

        importResult.Added.Should().Be(2);
        importResult.Updated.Should().Be(1);
        importResult.Failed.Should().Be(0);
        importResult.Skipped.Should().Be(3);

        importResult = await this.PostImportTest(this.itemsUrl, "commentsWithKeyUpdate.csv");

        importResult.Added.Should().Be(1);
        importResult.Updated.Should().Be(0);
        importResult.Failed.Should().Be(0);
        importResult.Skipped.Should().Be(5);

        importResult = await this.PostImportTest(this.itemsUrl, "commentsWithKeyUpdateWithErrors.csv");

        importResult.Added.Should().Be(1);
        importResult.Updated.Should().Be(1);
        importResult.Failed.Should().Be(2);
        importResult.Skipped.Should().Be(2);
    }

    protected override void MustBeEquivalent(object? expected, object? result)
    {
        result.Should().BeEquivalentTo(expected, options => options.Excluding(o => ((TestComment)o!).ContactId));
        ((Comment)result!).ContactId.Should().BePositive();
    }

    protected override async Task<(TestComment, string)> CreateItem(string uid, int fkId)
    {
        var testComment = new TestComment(uid, fkId);

        var newCommentUrl = await this.PostTest(this.itemsUrl, testComment);

        return (testComment, newCommentUrl);
    }

    protected override async Task<(int, string)> CreateFKItem(string authToken = "Success")
    {
        var fkItemCreate = new TestContent();

        var fkUrl = await this.PostTest("/api/content", fkItemCreate);

        var fkItem = await this.GetTest<Content>(fkUrl);

        fkItem.Should().NotBeNull();

        return (fkItem!.Id, fkUrl);
    }

    protected override CommentUpdateDto UpdateItem(TestComment to)
    {
        var from = new CommentUpdateDto();
        to.Body = from.Body = to.Body + "Updated";
        return from;
    }

    private async Task CreateFKItemsWithUid(int contactsCount = 2)
    {
        var fkItemCreate1 = new TestContent("100");
        var fkItemCreate2 = new TestContent("101");

        await this.PostTest("/api/content", fkItemCreate1);
        await this.PostTest("/api/content", fkItemCreate2);

        for (var i = 1; i <= contactsCount; i++)
        {
            var contact = new TestContact(i.ToString());
            await this.PostTest("/api/contacts", contact);
        }
    }
}