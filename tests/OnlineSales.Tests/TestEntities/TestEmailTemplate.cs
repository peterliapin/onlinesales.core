// <copyright file="TestEmailTemplate.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using OnlineSales.DTOs;

namespace OnlineSales.Tests.TestEntities;

public class TestEmailTemplate : EmailTemplateCreateDto
{
    public TestEmailTemplate(string uid = "", int groupId = 0)
    {
        Name = $"TestEmailTemplate{uid}";
        Subject = $"TestEmailTemaplteSubject{uid}";
        BodyTemplate = $"TestEmailTemaplteSubjectBodyTemplate{uid}";
        FromEmail = $"test{uid}@test.net";
        FromName = "TestEmailTemaplteFromName";
        Language = "en";
        GroupId = groupId;
    }
}