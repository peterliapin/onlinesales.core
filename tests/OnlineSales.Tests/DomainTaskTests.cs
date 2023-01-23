// <copyright file="DomainTaskTests.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using FluentAssertions;
using OnlineSales.DTOs;
using OnlineSales.Entities;
using OnlineSales.Helpers;

namespace OnlineSales.Tests;

public class DomainTaskTests : BaseTest
{   
    private readonly string tasksUrl = "/api/tasks";

    private readonly string domainsUrl = "/api/domains";

    private readonly string taskName = "DomainVerificationTask";

    [Fact]
    public async Task ExecuteTest()
    {
        var responce = await GetTest<TaskDetailsDto>(tasksUrl + "/stop/" + taskName);
        responce.Should().NotBeNull();
        responce!.IsRunning.Should().BeFalse();

        var validDomain = new TestDomain()
        {
            Name = "gmail.com",
        };

        var invalidDomain = new TestDomain()
        {
            Name = "SomeIncorrectDomainName",
        };

        var filledDomain = new TestDomain()
        {
            Name = "filledDomainName",
            HttpCheck = false,
            Url = "Url",
            Title = "Title",
            Description = "Description",
            DnsRecords = new List<DnsRecord>(3),
            DnsCheck = false,
        };

        var validDomainLocation = await PostTest(domainsUrl, validDomain);

        var invalidDomainLocation = await PostTest(domainsUrl, invalidDomain);

        var filledDomainLocation = await PostTest(domainsUrl, filledDomain);

        var validDomainAdded = await GetTest<Domain>(validDomainLocation);
        validDomainAdded.Should().NotBeNull();   
        validDomainAdded!.HttpCheck.Should().BeNull();
        validDomainAdded!.DnsCheck.Should().BeNull();

        var invalidDomainAdded = await GetTest<Domain>(invalidDomainLocation);
        invalidDomainAdded.Should().NotBeNull();
        invalidDomainAdded!.HttpCheck.Should().BeNull();
        invalidDomainAdded!.DnsCheck.Should().BeNull();

        var filledDomainAdded = await GetTest<Domain>(filledDomainLocation);
        filledDomainAdded.Should().BeEquivalentTo(filledDomain);

        var executeResponce = await GetTest<TaskExecutionDto>(tasksUrl + "/execute/" + taskName);
        executeResponce.Should().NotBeNull();
        executeResponce!.Name.Should().Be(taskName);
        executeResponce!.Completed.Should().BeTrue();

        validDomainAdded = await GetTest<Domain>(validDomainLocation);
        validDomainAdded.Should().NotBeNull();
        validDomainAdded!.HttpCheck.Should().BeTrue();
        validDomainAdded!.Url.Should().NotBeNull();
        validDomainAdded!.Title.Should().NotBeNull();
        validDomainAdded!.Description.Should().NotBeNull();
        validDomainAdded!.DnsCheck.Should().BeTrue();
        validDomainAdded!.DnsRecords.Should().NotBeNull();

        invalidDomainAdded = await GetTest<Domain>(invalidDomainLocation);
        invalidDomainAdded.Should().NotBeNull();
        invalidDomainAdded!.HttpCheck.Should().BeFalse();
        invalidDomainAdded!.Url.Should().BeNull();
        invalidDomainAdded!.Title.Should().BeNull();
        invalidDomainAdded!.Description.Should().BeNull();
        invalidDomainAdded!.DnsCheck.Should().BeFalse();
        invalidDomainAdded!.DnsRecords.Should().BeNull();

        filledDomainAdded = await GetTest<Domain>(filledDomainLocation);
        filledDomainAdded.Should().BeEquivalentTo(filledDomain);
    }
}