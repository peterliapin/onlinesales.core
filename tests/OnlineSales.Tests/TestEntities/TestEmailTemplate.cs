// <copyright file="TestEmailTemplate.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

namespace OnlineSales.Tests.TestEntities;

public class TestEmailTemplate : EmailTemplateCreateDto
{
    public TestEmailTemplate(string uid = "", int groupId = 0)
    {
        this.Name = $"TestEmailTemplate{uid}";
        this.Subject = $"TestEmailTemaplteSubject{uid}";
        this.BodyTemplate = $"TestEmailTemaplteSubjectBodyTemplate{uid}";
        this.FromEmail = $"test{uid}@test.net";
        this.FromName = "TestEmailTemaplteFromName";
        this.Language = "en";
        this.GroupId = groupId;
    }
}