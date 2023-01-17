// <copyright file="IContactService.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using OnlineSales.Entities;

namespace OnlineSales.Interfaces
{
    public interface IContactService
    {
        Task<Contact> AddContact(Contact contact);

        Task<Contact> UpdateContact(Contact contact);

        List<Contact> DomainMapperWithContacts(List<Contact> contacts);
    }
}
