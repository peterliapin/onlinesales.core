// <copyright file="TestContent.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

namespace OnlineSales.Tests.TestEntities;

public class TestContent : ContentCreateDto
{
    public TestContent(string uid = "")
    {
        this.Slug = $"test-slug{uid}";
        this.Type = "Test Template";
        this.Author = "Peter Liapin";
        this.Title = $"Test Title (via test suit){uid}";
        this.Description = $"This is a sample test description{uid}";
        this.Body = $"This is a sample Content{uid}";
        this.CoverImageUrl = $"/api/images/{this.Slug}/cover.png";
        this.CoverImageAlt = $"This is a sample Cover alt{uid}";
        this.Language = "en-US";
        this.Category = "Product";
        this.Tags = new string[] { "Tag1" };
        this.AllowComments = true;
    }
}