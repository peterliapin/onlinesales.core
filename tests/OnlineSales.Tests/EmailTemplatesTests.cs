// <copyright file="EmailTemplatesTests.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using FluentAssertions;
using OnlineSales.DTOs;
using OnlineSales.Entities;
using OnlineSales.Tests.TestEntities.BulkPopulate;

namespace OnlineSales.Tests;

public class EmailTemplatesTests : TableWithFKTests<EmailTemplate, TestEmailTemplate, EmailTemplateUpdateDto, EmailTemplateDetailsDto, TestBulkEmailTemplates>
{
    public EmailTemplatesTests()
        : base("/api/email-templates")
    {
    }

    protected override async Task<(TestEmailTemplate, string)> CreateItem(int fkId, Action<TestEmailTemplate>? itemTransformation = null)
    {
        var testEmailTemplate = new TestEmailTemplate
        {
            GroupId = fkId,
        };

        if (itemTransformation != null)
        {
            itemTransformation(testEmailTemplate);
        }

        var newEmailTemplateUrl = await PostTest(itemsUrl, testEmailTemplate);

        return (testEmailTemplate, newEmailTemplateUrl);
    }

    protected override async Task<(int, string)> CreateFKItem()
    {
        var fkItemCreate = new TestEmailGroup();

        var fkUrl = await PostTest("/api/email-groups", fkItemCreate);

        var fkItem = await GetTest<EmailGroup>(fkUrl);

        fkItem.Should().NotBeNull();

        return (fkItem!.Id, fkUrl);
    }

    protected override EmailTemplateUpdateDto UpdateItem(TestEmailTemplate to)
    {
        var from = new EmailTemplateUpdateDto();
        to.Name = from.Name = to.Name + "Updated";
        return from;
    }
}