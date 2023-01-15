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
}