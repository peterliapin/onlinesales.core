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
            Dictionary<string, Domain> newDomains = new Dictionary<string, Domain>();

            var contactsWithDomain = from contact in contacts select new { Contact = contact, DomainName = GetDomainFromEmail(contact.Email) };

            var domainsQueryResult = (from contactWithDomain in contactsWithDomain
                                      join domain in apiDbContext.Domains! on contactWithDomain.DomainName equals domain.Name into domainTemp
                                      from domain in domainTemp.DefaultIfEmpty()
                                      select new { EnteredContact = contactWithDomain.Contact, DomainName = contactWithDomain.DomainName, DomainId = domain?.Id ?? 0 }).ToList();

            foreach (var domainItem in domainsQueryResult)
            {
                if (domainItem.DomainId != 0)
                {
                    domainItem.EnteredContact.DomainId = domainItem.DomainId;
                }
                else
                {
                    var domain = new Domain() { Name = domainItem.DomainName };

                    var existingDomain = from domainDictionary in newDomains where domainDictionary.Key == domain.Name select domainDictionary;

                    if (!existingDomain.Any())
                    {
                        newDomains.Add(domain.Name, domain);
                        apiDbContext.Add(domain);
                        domainItem.EnteredContact.Domain = domain;
                    }
                    else
                    {
                        domainItem.EnteredContact.Domain = existingDomain.FirstOrDefault().Value;
                    }
                }
            }

            var transformedContacts = (from dq in domainsQueryResult select dq.EnteredContact).ToList();

            return transformedContacts;
        }

        public string GetDomainFromEmail(string email)
        {
            return email.Split("@").Last().ToString();
        }
    }
}
