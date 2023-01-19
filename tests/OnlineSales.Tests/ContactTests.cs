// <copyright file="ContactTests.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using FluentAssertions;
using OnlineSales.DTOs;
using OnlineSales.Entities;

namespace OnlineSales.Tests;
public class ContactTests : SimpleTableTests<Contact, TestContact, ContactUpdateDto>
{
    public ContactTests()
        : base("/api/contacts")
    {
    }

    [Fact]
    public async Task CheckInsertedItemDomain()
    {
        var testCreateItem = await CreateItem();

        var returnedDomain = DomainChecker(testCreateItem.Item1.Email);
        returnedDomain.Should().NotBeNull();
    }

    [Theory]
    [InlineData("contacts.json")]
    public async Task ImportFileAddCheckDomain(string fileName)
    {
        await PostImportTest(itemsUrl, fileName);

        var newContact = await GetTest<Contact>($"{itemsUrl}/2");
        newContact.Should().NotBeNull();

        var returnedDomain = DomainChecker(newContact!.Email);
        returnedDomain.Should().NotBeNull();
    }

    protected override ContactUpdateDto UpdateItem(TestContact to)
    {
        var from = new ContactUpdateDto();
        to.Email = from.Email = "Updated" + to.Email;
        return from;
    }

    protected override void GenerateBulkRecords(int dataCount, Action<TestContact>? populateAttributes = null)
    {
        List<TestContact> testContacts = new List<TestContact>();

        for (int i = 0; i < dataCount; i++)
        {
             var contact = new TestContact(i.ToString());
             contact.Domain = new Domain() { Name = contact.Email.Split("@").Last() };

             testContacts.Add(contact);
        }

        var dbData = mapper.Map<List<Contact>>(testContacts);

        App.PopulateBulkData(dbData);
    }

    private string DomainChecker(string email)
    {
        var domain = email.Split("@").Last().ToString();
        var dbContext = App.GetDbContext();
        var dbDomain = dbContext!.Domains!.Where(domainDb => domainDb.Name == domain).Select(domainDb => domainDb.Name).FirstOrDefault();

        return dbDomain!;
    }
}