// <copyright file="EmailTemplatesTests.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using FluentAssertions;
using OnlineSales.DTOs;
using OnlineSales.Entities;

namespace OnlineSales.Tests;

public class EmailTemplatesTests : BaseTest
{    
    private readonly string urlEmailTemplates = "/api/email-templates";
    private readonly string urlEmailTemplatesNotFound = "/api/email-templates" + "/404";

    [Fact]
    public async Task GetEmailTemplateNotFoundTest()
    {
        await GetTest(urlEmailTemplatesNotFound, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateEmailTemplateWithNonExistedEmailGroupTest()
    {
        var testEmailTemplate = new TestEmailTemplate();
        testEmailTemplate.GroupId = 1; // non existing group id (because each test starts with empty database)
        await PostTest(urlEmailTemplates, testEmailTemplate, HttpStatusCode.InternalServerError);
    }
        
    [Fact]
    public async Task CreateAndGetEmailTemplateTest()
    {
        var testEmailTemplate = AddEmailGroupAndCreateEmailTemplate();

        var newEmailTemplateUrl = await PostTest(urlEmailTemplates, testEmailTemplate);

        var emailTemplate = await GetTest<EmailTemplate>(newEmailTemplateUrl);

        emailTemplate.Should().BeEquivalentTo(testEmailTemplate);
    }
    
    [Fact]
    public async Task UpdateEmailTemplateNotFoundTest()
    {
        await PatchTest(urlEmailTemplatesNotFound, new { }, HttpStatusCode.NotFound);
    }
    
    [Fact]
    public async Task CreateAndUpdateEmailTemplateNameTest()
    {
        var testEmailTemplate = AddEmailGroupAndCreateEmailTemplate();

        var newEmailTemplateUrl = await PostTest(urlEmailTemplates, testEmailTemplate);

        var updatedEmailTemplate = new EmailTemplateUpdateDto();

        testEmailTemplate.Name = updatedEmailTemplate.Name = "Update EmailTemplate Name";

        await PatchTest(newEmailTemplateUrl, updatedEmailTemplate);

        var emailTemplate = await GetTest<EmailTemplate>(newEmailTemplateUrl);

        emailTemplate.Should().BeEquivalentTo(testEmailTemplate);
    }

    [Fact]
    public async Task DeleteEmailTemplateNotFoundTest()
    {
        await DeleteTest(urlEmailTemplatesNotFound, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateAndDeleteEmailTemplateTest()
    {
        var testEmailTemplate = AddEmailGroupAndCreateEmailTemplate();

        var newEmailTemplateUrl = await PostTest(urlEmailTemplates, testEmailTemplate);

        await DeleteTest(newEmailTemplateUrl);

        await GetTest(newEmailTemplateUrl, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteEmailGroupAndAllEmailTemplatesTest()
    {
        var emailGroup = AddEmailGroup();

        var emailGroupId = emailGroup.Item1;

        int numberOfEmailTemplates = 10;

        string[] emailTemplatesUrls = new string[numberOfEmailTemplates];

        for (var i = 0; i < numberOfEmailTemplates; ++i)
        {
            var testEmailTemplate = new TestEmailTemplate();
            testEmailTemplate.GroupId = emailGroupId;

            var newEmailTemplateUrl = await PostTest(urlEmailTemplates, testEmailTemplate);

            emailTemplatesUrls[i] = newEmailTemplateUrl;
        }

        await DeleteTest(emailGroup.Item2);

        for (var i = 0; i < numberOfEmailTemplates; ++i)
        {
            await GetTest<EmailGroup>(emailTemplatesUrls[i], HttpStatusCode.NotFound);
        }
    }

    private (int, string) AddEmailGroup()
    {
        var testEmailGroup = new TestEmailGroupCreate();

        var emailGroupUrl = PostTest("/api/email-groups", testEmailGroup).Result;

        var emailGroup = GetTest<EmailGroup>(emailGroupUrl).Result;

        emailGroup.Should().NotBeNull();

        return (emailGroup == null ? 0 : emailGroup.Id, emailGroupUrl);
    }

    private TestEmailTemplate AddEmailGroupAndCreateEmailTemplate()
    {
        var addedEmailGroup = AddEmailGroup();

        var emailGroupId = addedEmailGroup.Item1;

        var testEmailTemplate = new TestEmailTemplate
        {
            GroupId = emailGroupId,
        };

        return testEmailTemplate;
    }
}