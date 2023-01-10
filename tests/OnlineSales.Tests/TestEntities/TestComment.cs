// <copyright file="TestComment.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using OnlineSales.DTOs;

namespace OnlineSales.Tests.TestEntities;

public class TestComment : CommentCreateDto
{
    public TestComment(string uid = "", int postId = 0)
    {
        AuthorName = $"Test Author{uid}";
        AuthorEmail = $"author{uid}@test.email";
        Content = $"Test Comment{uid}";
        PostId = postId;
    }
}