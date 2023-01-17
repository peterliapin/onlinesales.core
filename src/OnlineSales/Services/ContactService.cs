// <copyright file="ContactService.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using OnlineSales.Data;
using OnlineSales.Entities;
using OnlineSales.Interfaces;

namespace OnlineSales.Services
{
    public class ContactService : IContactService
    {
        private readonly ApiDbContext apiDbContext;

        public ContactService(ApiDbContext apiDbContext)
        {
            this.apiDbContext = apiDbContext;
        }

        public async Task<Contact> AddContact(Contact contact)
        {
            var contactWrappWithList = new List<Contact>()
            { contact };

            var returnedValue = DomainMapperWithContacts(contactWrappWithList);

            await apiDbContext.Contacts!.AddRangeAsync(returnedValue);

            await apiDbContext.SaveChangesAsync();

            return returnedValue!.FirstOrDefault() !;
        }

        public async Task<Contact> UpdateContact(Contact contact)
        {
            var contactWrappWithList = new List<Contact>()
            { contact };

            var returnedValue = DomainMapperWithContacts(contactWrappWithList);

            apiDbContext.Contacts!.UpdateRange(returnedValue);

            await apiDbContext.SaveChangesAsync();

            return contactWrappWithList!.FirstOrDefault() !;
        }

        public List<Contact> DomainMapperWithContacts(List<Contact> contacts)
        {
            var domainsQueryResult = (from ed in contacts
                                      join dom in apiDbContext.Domains! on ed.Email.Split("@").Last() equals dom.Name into domTm
                                      from dom in domTm.DefaultIfEmpty()
                                      select new { EnteredDomain = ed, DomId = dom?.Id ?? 0 }).ToList();

            foreach (var item in domainsQueryResult!.Where(dr => dr.DomId == 0).Select(ed => ed.EnteredDomain))
            {
                item.Domain = new Domain() { Name = item.Email.Split("@").Last() };
            }

            foreach (var item in domainsQueryResult!.Where(dr => dr.DomId != 0))
            {
                item.EnteredDomain.DomainId = item.DomId;
            }

            var transformedContacts = (from dq in domainsQueryResult select dq.EnteredDomain).ToList();

            return transformedContacts;
        }
    }
}
