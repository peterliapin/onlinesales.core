// <copyright file="DomainTests.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

namespace OnlineSales.Tests;
public class DomainTests : SimpleTableTests<Domain, TestDomain, DomainUpdateDto, IDomainService>
{
    public DomainTests()
        : base("/api/domains")
    {
    }

    [Fact]
    public async Task CreateAndGetItemByNameTest()
    {
        var testDomain = new TestDomain();

        var domainName = testDomain.Name;

        await PostTest(itemsUrl, testDomain);

        var url = itemsUrl + "/verify/" + domainName;

        var item = await GetTest<Domain>(url, HttpStatusCode.OK);

        item.Should().NotBeNull();
    }

    [Fact]
    public async Task GetNewValidDomainTest()
    {
        var domainName = "gmail.com";

        var url = itemsUrl + "/verify/" + domainName;

        var item = await GetTest<Domain>(url, HttpStatusCode.OK);

        item.Should().NotBeNull();

        item!.Name.Should().Be(domainName);
        item!.DnsCheck.Should().BeTrue();
        item!.DnsRecords.Should().NotBeNull();
        item!.HttpCheck.Should().BeTrue();
        item!.Url.Should().NotBeNull();
        item!.Title.Should().NotBeNull();
        item!.Description.Should().NotBeNull();
    }

    [Fact]
    public async Task GetExistedValidDomainTest()
    {
        var testDomain = new TestDomain();

        var domainName = testDomain.Name;
        domainName.Should().NotBeEmpty();

        var location = await PostTest(itemsUrl, testDomain);

        var item = await GetTest<Domain>(location, HttpStatusCode.OK);

        item.Should().NotBeNull();
        item!.Name.Should().Be(domainName);
        item!.DnsCheck.Should().BeNull();
        item!.DnsRecords.Should().BeNull();
        item!.HttpCheck.Should().BeNull();
        item!.Url.Should().BeNull();
        item!.Title.Should().BeNull();
        item!.Description.Should().BeNull();

        var url = itemsUrl + "/verify/" + domainName;

        item = await GetTest<Domain>(url, HttpStatusCode.OK);

        item.Should().NotBeNull();
        item!.Name.Should().Be(domainName);
        item!.DnsCheck.Should().BeTrue();
        item!.DnsRecords.Should().NotBeNull();
        item!.HttpCheck.Should().BeTrue();
        item!.Url.Should().NotBeNull();
        item!.Title.Should().NotBeNull();
        item!.Description.Should().NotBeNull();
    }

    [Fact]
    public async Task GetNewInvalidDomainTest()
    {
        var domainName = "incorrect-domain";

        var url = itemsUrl + "/verify/" + domainName;

        var item = await GetTest<Domain>(url, HttpStatusCode.OK);

        item.Should().NotBeNull();
        item!.Name.Should().Be(domainName);
        item!.DnsCheck.Should().BeFalse();
        item!.DnsRecords.Should().BeNull();
        item!.HttpCheck.Should().BeFalse();
        item!.Url.Should().BeNull();
        item!.Title.Should().BeNull();
        item!.Description.Should().BeNull();
    }

    protected override DomainUpdateDto UpdateItem(TestDomain to)
    {
        var from = new DomainUpdateDto();
        to.Description = from.Description = "new-" + to.Description;
        return from;
    }
}