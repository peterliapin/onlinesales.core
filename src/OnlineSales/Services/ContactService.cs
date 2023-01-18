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
            var returnedValue = EnrichWithDomainId(contact);

            await apiDbContext.Contacts!.AddAsync(returnedValue);

            await apiDbContext.SaveChangesAsync();

            return returnedValue!;
        }

        public async Task<Contact> UpdateContact(Contact contact)
        {
            var returnedValue = EnrichWithDomainId(contact);

            apiDbContext.Contacts!.Update(returnedValue);

            await apiDbContext.SaveChangesAsync();

            return returnedValue;
        }

        public Contact EnrichWithDomainId(Contact contact)
        {
            var domainName = GetDomainFromEmail(contact.Email);

            var domainsQueryResult = apiDbContext!.Domains!.Where(domain => domain.Name == domainName).FirstOrDefault();

            if (domainsQueryResult != null)
            {
                contact.DomainId = domainsQueryResult.Id;
            }
            else
            {
                contact.Domain = new Domain() { Name = domainName };
            }

            return contact;
        }

        public List<Contact> EnrichWithDomainId(List<Contact> contacts)
        {
            var domainsQueryResult = (from contact in contacts
                                      join domain in apiDbContext.Domains! on GetDomainFromEmail(contact.Email) equals domain.Name into domainTemp
                                      from domain in domainTemp.DefaultIfEmpty()
                                      select new { EnteredDomain = contact, DomainId = domain?.Id ?? 0 }).ToList();

            foreach (var domainItem in domainsQueryResult)
            {
                if (domainItem.DomainId != 0)
                {
                    domainItem.EnteredDomain.DomainId = domainItem.DomainId;
                }
                else
                {
                    domainItem.EnteredDomain.Domain = new Domain() { Name = GetDomainFromEmail(domainItem.EnteredDomain.Email) };
                }
            }

            var transformedContacts = (from dq in domainsQueryResult select dq.EnteredDomain).ToList();

            return transformedContacts;
        }

        public string GetDomainFromEmail(string email)
        {
            return email.Split("@").Last().ToString();
        }
    }
}
