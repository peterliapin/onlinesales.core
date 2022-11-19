// <copyright file="TestComment.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel.DataAnnotations;
using OnlineSales.DTOs;

namespace OnlineSales.Tests.TestEntities;

public class TestCommentCreateDto : CommentCreateDto
{
    public TestCommentCreateDto()
    {
        AuthorName = "Test Author";
        AuthorEmail = "author@test.email";
        Content = "Test Comment";
    }
}