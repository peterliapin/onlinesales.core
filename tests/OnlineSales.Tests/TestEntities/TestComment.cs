// <copyright file="TestComment.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

namespace OnlineSales.Tests.TestEntities;

public class TestComment : CommentCreateDto
{
    public TestComment(string uid = "", int commentableId = 0)
    {
        AuthorName = $"Test Author{uid}";
        AuthorEmail = $"contact{uid}@test{uid}.net";
        Body = $"Test Comment{uid}";
        CommentableId = commentableId;
        CommentableType = CommentableType.Content;
    }
}