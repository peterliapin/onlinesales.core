// <copyright file="PostsTests.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using FluentAssertions;
using OnlineSales.DTOs;
using OnlineSales.Entities;

namespace OnlineSales.Tests;

public class PostsTests : SimpleTableTests<Post, TestPostCreateDto, TestPostUpdateDto>
{
    public PostsTests()
        : base("/api/posts")
    {
    }

    protected override TestPostUpdateDto UpdateItem(TestPostCreateDto to)
    {
        var from = new TestPostUpdateDto();
        to.Slug = from.Slug ?? to.Slug;
        to.Template = from.Template ?? to.Template;
        to.Author = from.Author ?? to.Author;
        to.Title = from.Title ?? to.Title;
        to.Description = from.Description ?? to.Description;
        to.Content = from.Content ?? to.Content;
        to.CoverImageAlt = from.CoverImageAlt ?? to.CoverImageAlt;
        to.CoverImageUrl = from.CoverImageUrl ?? to.CoverImageUrl;
        to.Language = from.Language ?? to.Language;
        to.Categories = from.Categories ?? to.Categories;
        to.Tags = from.Tags ?? to.Tags;
        to.AllowComments = from.AllowComments ?? to.AllowComments;
        return from;
    }
}

