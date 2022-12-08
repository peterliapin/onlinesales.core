// <copyright file="EmailTemplatesTests.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using FluentAssertions;
using Namotion.Reflection;
using OnlineSales.DTOs;
using OnlineSales.Entities;

namespace OnlineSales.Tests;

public class EmailTemplatesTests : BaseTest
{
    private readonly string urlEmailTemplates = "/api/emailtemplates";
    private readonly string urlEmailTemplatesNotFound = "/api/emailtemplates" + "/404";

    [Fact]
    public async Task GetEmailTemplateNotFoundTest()
    {
        await GetTest(urlEmailTemplatesNotFound, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateAndGetEmailTemplateTest()
    {
        var testEmailTemplate = new TestEmailTemplate();

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
        var testEmailTemplate = new TestEmailTemplate();

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
    public async Task CreateAndDeletePostTest()
    {
        var testEmailTemplate = new TestEmailTemplate();

        var newEmailTemplateUrl = await PostTest(urlEmailTemplates, testEmailTemplate);

        await DeleteTest(newEmailTemplateUrl);

        await GetTest(newEmailTemplateUrl, HttpStatusCode.NotFound);
    }
}