// <copyright file="ContentTests.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using OnlineSales.Helpers;

namespace OnlineSales.Tests;

public class ContentTests : SimpleTableTests<Content, TestContent, ContentUpdateDto, ISaveService<Content>>
{
    public ContentTests()
        : base("/api/content")
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
    public async Task CheckTags()
    {
        await this.CreateItem();
        var response = await this.GetTest(this.itemsUrl + "/tags", HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var data = JsonHelper.Deserialize<string[]>(content);
        data.Should().NotBeNull();
        data.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CheckCategories()
    {
        await this.CreateItem();
        var response = await this.GetTest(this.itemsUrl + "/categories", HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var data = JsonHelper.Deserialize<string[]>(content);
        data.Should().NotBeNull();
        data.Should().NotBeEmpty();
    }

    protected override ContentUpdateDto UpdateItem(TestContent to)
    {
        var from = new ContentUpdateDto();
        to.Type = from.Type = to.Type + "Updated";
        return from;
    }
}