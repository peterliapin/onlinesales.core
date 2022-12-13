// <copyright file="EmailTemplatesTests.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using FluentAssertions;
using OnlineSales.DTOs;
using OnlineSales.Entities;

namespace OnlineSales.Tests;

public class EmailTemplatesTests : TableWithFKTests<EmailTemplate, TestEmailTemplateCreate, TestEmailTemplateUpdate, EmailGroup, TestEmailGroupCreate>
{
    public EmailTemplatesTests()
        : base("/api/email-templates", "/api/email-groups")
    {
    }

    protected override async Task<(TestEmailTemplateCreate, string)> CreateItem(int fkId)
    {
        var testEmailTemplate = new TestEmailTemplateCreate
        {
            GroupId = fkId,
        };

        var newEmailTemplateUrl = await PostTest(itemsUrl, testEmailTemplate);

        return (testEmailTemplate, newEmailTemplateUrl);
    }

    protected override void ChangeFKId(TestEmailTemplateUpdate item, int fkId)
    {
        item.GroupId = fkId;
    }

    protected override TestEmailTemplateUpdate UpdateItem(TestEmailTemplateCreate to)
    {
        var from = new TestEmailTemplateUpdate();
        to.Name = from.Name ?? to.Name;
        to.Subject = from.Subject ?? to.Subject;
        to.BodyTemplate = from.BodyTemplate ?? to.BodyTemplate;
        to.FromEmail = from.FromEmail ?? to.FromEmail;
        to.FromName = from.FromName ?? to.FromName;
        return from;
    }
}