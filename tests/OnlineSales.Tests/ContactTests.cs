// <copyright file="ContactTests.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System;
using System.Net.Http.Headers;
using System.Security.Policy;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using FluentAssertions.Equivalency.Steps;
using Nest;
using OnlineSales.DTOs;
using OnlineSales.Entities;
using OnlineSales.Helpers;

namespace OnlineSales.Tests;
public class ContactTests : SimpleTableTests<Contact, TestContact, ContactUpdateDto>
{
    public ContactTests()
        : base("/api/contacts")
    {
    }

    [Fact]
    public async Task ContactAccountTaskTest()
    {
        var notinitializedEmail = "email@notinitializeddomain.com";

        // posted contacts marked as notInitialized and the details of their accounts should be retrieved
        var item = TestData.Generate<TestContact>();
        item.Email = notinitializedEmail;
        await PostTest(itemsUrl, item);

        // imported contacts marked as notIntended and the details of their accounts shouldn't be retrieved
        await PostImportTest(itemsUrl, "contacts.json");

        HttpResponseMessage executeResponce = await GetRequest("/api/tasks/execute/ContactAccountTask");
        executeResponce.StatusCode.Should().Be(HttpStatusCode.OK);

        var contacts = App.GetDbContext() !.Contacts!.ToList();
        contacts.Count.Should().BeGreaterThan(1);

        foreach (var contact in contacts)
        {
            var domain = App.GetDbContext() !.Domains!.Where(d => d.Id == contact!.DomainId).FirstOrDefault();
            domain.Should().NotBeNull();
            if (contact.Email == notinitializedEmail)
            {
                domain !.AccountStatus.Should().Be(AccountSyncStatus.Successful);
                domain.AccountId.Should().NotBeNull();
            }
            else
            {
                domain!.AccountStatus.Should().Be(AccountSyncStatus.NotIntended);
                domain.AccountId.Should().BeNull();
            }

            domain.AccountId.Should().Be(contact.AccountId);
        }
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

    [Fact]
    public async Task ImportFileUpdateByIndexTest()
    {
        await PostImportTest(itemsUrl, "contactBase.csv");
        var allContactsResponse = await GetTest(itemsUrl);
        allContactsResponse.Should().NotBeNull();

        var content = await allContactsResponse.Content.ReadAsStringAsync();
        var allContacts = JsonSerializer.Deserialize<List<Contact>>(content);
        allContacts.Should().NotBeNull();
        allContacts!.Count.Should().Be(4);

        await PostImportTest(itemsUrl, "contactsToUpdate.csv");
        var contact1 = App.GetDbContext() !.Contacts!.First(c => c.Id == 1);
        contact1.Should().NotBeNull();
        // contact1 updated by Id
        contact1.LastName.Should().Be("adam_parker");

        var contact2 = App.GetDbContext() !.Contacts!.First(c => c.Id == 2);
        contact2.Should().NotBeNull();
        // contact2 no id provided, updated by Email
        contact2.LastName.Should().Be("garry_bolt");

        var contact3 = App.GetDbContext() !.Contacts!.First(c => c.Id == 3);
        contact3.Should().NotBeNull();
        // contact3 no id provided, updated by Email
        contact3.LastName.Should().Be("Siri_Tom");
    }

    [Fact]
    public async Task ParentDeletionRestrictWithChildren()
    {
        int contactId = 0;

        var testCreateItem = await CreateItem();

        contactId = Convert.ToInt32(testCreateItem.Item2.Split("/").Last());

        var dbContext = App.GetDbContext();
        var dbDomainId = dbContext!.Contacts!.Where(contactsDb => contactsDb.Id == contactId).Select(contact => contact.DomainId).FirstOrDefault();

        await DeleteTest($"/api/domains/{dbDomainId}");
    }

    protected override ContactUpdateDto UpdateItem(TestContact to)
    {
        var from = new ContactUpdateDto();
        to.Email = from.Email = "updated" + to.Email;
        return from;
    }

    protected override void GenerateBulkRecords(int dataCount, Action<TestContact>? populateAttributes = null)
    {
        var contacts = new List<Contact>();

        for (int i = 0; i < dataCount; i++)
        {
            var contact = new Contact();
            contact.Email = $"contact{i}@test{i}.net";
            contact.Domain = new Domain() { Name = contact.Email.Split("@").Last().ToLower() };
            contacts.Add(contact);
        }

        App.PopulateBulkData(contacts);
    }

    private string DomainChecker(string email)
    {
        var domain = email.Split("@").Last().ToString();
        var dbContext = App.GetDbContext();
        var dbDomain = dbContext!.Domains!.Where(domainDb => domainDb.Name == domain).Select(domainDb => domainDb.Name).FirstOrDefault();

        return dbDomain!;
    }
}