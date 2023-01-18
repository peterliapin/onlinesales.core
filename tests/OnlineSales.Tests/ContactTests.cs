// <copyright file="ContactTests.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using OnlineSales.DTOs;
using OnlineSales.Entities;

namespace OnlineSales.Tests;
public class ContactTests : SimpleTableTests<Contact, TestContact, ContactUpdateDto>
{
    public ContactTests()
        : base("/api/contacts")
    {
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
}