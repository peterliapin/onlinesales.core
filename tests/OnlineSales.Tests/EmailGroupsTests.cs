// <copyright file="EmailGroupsTests.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using FluentAssertions;
using OnlineSales.DTOs;
using OnlineSales.Entities;

namespace OnlineSales.Tests;

public class EmailGroupsTests : BaseTest
{
    private static readonly string UrlEmailGroups = "/api/email-groups";
    private static readonly string UrlEmailGroupsNotFound = UrlEmailGroups + "/404";

    [Fact]
    public async Task GetEmailGroupNotFoundTest()
    {
        await GetTest(UrlEmailGroupsNotFound, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateAndGetEmailGroupTest()
    {
        var testEmailGroup = new TestEmailGroup();

        var newEmailGroupUrl = await PostTest(UrlEmailGroups, testEmailGroup);

        var emailGroup = await GetTest<EmailGroup>(newEmailGroupUrl);

        emailGroup.Should().BeEquivalentTo(testEmailGroup);
    }

    [Fact]
    public async Task UpdateEmailGroupNotFoundTest()
    {
        await PatchTest(UrlEmailGroupsNotFound, new { }, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateAndUpdateEmailGroupNameTest()
    {
        var testEmailGroup = new TestEmailGroup();

        var newEmailGroupUrl = await PostTest(UrlEmailGroups, testEmailGroup);

        var updatedEmailGroup = new EmailGroupUpdateDto();

        testEmailGroup.Name = updatedEmailGroup.Name = "EmailGroupUpdatedName";

        await PatchTest(newEmailGroupUrl, updatedEmailGroup);

        var emailGroup = await GetTest<EmailTemplate>(newEmailGroupUrl);

        emailGroup.Should().BeEquivalentTo(testEmailGroup);
    }

    [Fact]
    public async Task DeleteEmailGroupNotFoundTest()
    {
        await DeleteTest(UrlEmailGroupsNotFound, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateAndDeleteEmailGroupTest()
    {
        var testEmailGroup = new TestEmailGroup();

        var newEmailGroupUrl = await PostTest(UrlEmailGroups, testEmailGroup);

        await DeleteTest(newEmailGroupUrl);

        await GetTest(newEmailGroupUrl, HttpStatusCode.NotFound);
    }
}