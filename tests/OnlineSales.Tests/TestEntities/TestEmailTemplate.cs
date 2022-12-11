// <copyright file="TestEmailTemplate.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using OnlineSales.DTOs;

namespace OnlineSales.Tests.TestEntities;

public class TestEmailTemplate : EmailTemplateCreateDto
{
    public TestEmailTemplate()
    {
        Name = "TestEmailTemplate";
        Subject = "TestEmailTemaplteSubject";
        BodyTemplate = "TestEmailTemaplteSubjectBodyTemplate";
        FromEmail = "test@test.net";
        FromName = "TestEmailTemaplteFromName";
    }
}