// <copyright file="TestComment.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

namespace OnlineSales.Tests.TestEntities;

public class TestComment : CommentCreateDto
{
    public TestComment(string uid = "", int contentId = 0)
    {
        AuthorName = $"Test Author{uid}";
        AuthorEmail = $"author{uid}@test.email";
        Body = $"Test Comment{uid}";
        ContentId = contentId;
    }
}