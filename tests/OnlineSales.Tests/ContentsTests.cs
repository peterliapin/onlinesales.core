// <copyright file="ContentsTests.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using OnlineSales.DTOs;
using OnlineSales.Entities;

namespace OnlineSales.Tests;

public class ContentsTests : SimpleTableTests<Content, TestContent, ContentUpdateDto>
{
    public ContentsTests()
        : base("/api/contents")
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

    protected override ContentUpdateDto UpdateItem(TestContent to)
    {
        var from = new ContentUpdateDto();
        to.Template = from.Template = to.Template + "Updated";
        return from;
    }
}