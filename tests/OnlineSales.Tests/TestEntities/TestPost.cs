// <copyright file="TestPost.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using OnlineSales.DTOs;

namespace OnlineSales.Tests.TestEntities;

public class TestPost : PostCreateDto
{
    public TestPost(string uid = "")
    {
        Slug = $"test-slug{uid}";
        Template = "Test Template";
        Author = "Peter Liapin";
        Title = $"Test Title (via test suit){uid}";
        Description = $"This is a sample test description{uid}";
        Content = $"This is a sample Content{uid}";
        CoverImageUrl = $"/api/images/{Slug}/cover.png";
        CoverImageAlt = $"This is a sample Cover alt{uid}";
        Language = "en-US";
        Categories = "Product";
        Tags = "Tag1";
        AllowComments = true;
    }
}