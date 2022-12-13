// <copyright file="TestEmailTemplate.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Security.Policy;
using OnlineSales.DTOs;

namespace OnlineSales.Tests.TestEntities;

public class TestEmailTemplateCreate : EmailTemplateCreateDto
{
    public TestEmailTemplateCreate()
    {
        Name = "TestEmailTemplate";
        Subject = "TestEmailTemaplteSubject";
        BodyTemplate = "TestEmailTemaplteSubjectBodyTemplate";
        FromEmail = "test@test.net";
        FromName = "TestEmailTemaplteFromName";
        GroupId = 1;
    }
}

public class TestEmailTemplateUpdate : EmailTemplateUpdateDto
{
    public TestEmailTemplateUpdate()
    {
        Name = "TestEmailTemplateUpdated";
        Subject = "TestEmailTemaplteSubjectUpdated";
        BodyTemplate = "TestEmailTemaplteSubjectBodyTemplateUpdated";
        FromEmail = "testUpdated@test.net";
        FromName = "TestEmailTemaplteFromNameUpdated";
    }
}