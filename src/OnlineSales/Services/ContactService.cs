// <copyright file="ContactService.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using Microsoft.EntityFrameworkCore;

using OnlineSales.Data;
using OnlineSales.Entities;
using OnlineSales.Interfaces;

namespace OnlineSales.Services
{
    public class ContactService : IContactService
    {
        private readonly IDomainService domainService;
        private readonly IContactIpService contactIpService;
        private PgDbContext pgDbContext;

        public ContactService(PgDbContext pgDbContext, IDomainService domainService, IContactIpService contactIpService)
        {
            this.pgDbContext = pgDbContext;
            this.domainService = domainService;
            this.contactIpService = contactIpService;
        }

        public async Task SaveAsync(Contact contact)
        {
            await EnrichWithDomainId(contact);
            EnrichWithAccountId(contact);
            await SaveIp(contact);

            if (contact.Id > 0)
            {
                pgDbContext.Contacts!.Update(contact);
            }
            else
            {
                await pgDbContext.Contacts!.AddAsync(contact);
            }
        }

        public async Task SaveRangeAsync(List<Contact> contacts)
        {
            await EnrichWithDomainIdAsync(contacts);
            EnrichWithAccountId(contacts);
            await SaveIpRange(contacts);

            var sortedContacts = contacts.GroupBy(c => c.Id > 0);

            foreach (var group in sortedContacts)
            {
                if (group.Key)
                {
                    pgDbContext.UpdateRange(group.ToList());
                }
                else
                {
                    await pgDbContext.AddRangeAsync(group.ToList());
                }
            }
        }

        public async Task Unsubscribe(string email, string reason, string source, DateTime createdAt, string? ip)
        {
            var contact = (from u in pgDbContext.Contacts
                           where u.Email == email
                           select u).FirstOrDefault();

            if (contact != null)
            {
                var unsubscribe = new Unsubscribe
                {
                    ContactId = contact.Id,
                    Reason = reason,
                    CreatedByIp = ip,
                    Source = source,
                    CreatedAt = createdAt,
                };

                await pgDbContext.Unsubscribes!.AddAsync(unsubscribe);

                contact.Unsubscribe = unsubscribe;
            }
        }

        public void SetDBContext(PgDbContext pgDbContext)
        {
            this.pgDbContext = pgDbContext;
            domainService.SetDBContext(pgDbContext);
            contactIpService.SetDBContext(pgDbContext);
        }

        /// <summary>
        /// Update <see cref="Contact.LastIp"/> and return new <see  langword="IpAddress"/> back.
        /// </summary>
        /// <param name="contact">Entity to update.</param>
        public string? UpdateContactIp(Contact contact)
        {
            return contactIpService.UpdateContactIp(contact);
        }

        private async Task EnrichWithDomainId(Contact contact)
        {
            var domainName = domainService.GetDomainNameByEmail(contact.Email);

            var domainsQueryResult = await pgDbContext!.Domains!.Where(domain => domain.Name == domainName).FirstOrDefaultAsync();

            if (domainsQueryResult != null)
            {
                contact.DomainId = domainsQueryResult.Id;
                contact.Domain = domainsQueryResult;
            }
            else
            {
                contact.Domain = new Domain()
                {
                    Name = domainName,
                    AccountStatus = AccountSyncStatus.NotInitialized,
                };

                await domainService.SaveAsync(contact.Domain);
            }
        }

        private async Task EnrichWithDomainIdAsync(List<Contact> contacts)
        {
            var newDomains = new Dictionary<string, Domain>();

            var contactsWithDomain = from contact in contacts
                                     select new
                                     {
                                         Contact = contact,
                                         DomainName = domainService.GetDomainNameByEmail(contact.Email),
                                     };

            try
            {
                var contactsWithDomainInfo = (from contactWithDomain in contactsWithDomain
                                              join domain in pgDbContext.Domains! on contactWithDomain.DomainName equals domain.Name into domainTemp
                                              from domain in domainTemp.DefaultIfEmpty()
                                              select new
                                              {
                                                  Contact = contactWithDomain.Contact,
                                                  DomainName = contactWithDomain.DomainName,
                                                  Domain = domain,
                                                  DomainId = domain?.Id ?? 0,
                                              }).ToList();

                foreach (var contactWithDomainInfo in contactsWithDomainInfo)
                {
                    if (contactWithDomainInfo.DomainId != 0)
                    {
                        contactWithDomainInfo.Contact.DomainId = contactWithDomainInfo.DomainId;
                        contactWithDomainInfo.Contact.Domain = contactWithDomainInfo.Domain;
                    }
                    else
                    {
                        var existingDomain = from newDomain in newDomains
                                             where newDomain.Key == contactWithDomainInfo.DomainName
                                             select newDomain;

                        if (existingDomain.Any() is false)
                        {
                            var domain = new Domain()
                            {
                                Name = contactWithDomainInfo.DomainName,
                                Source = contactWithDomainInfo.Contact.Email,
                                AccountStatus = AccountSyncStatus.NotIntended,
                            };

                            newDomains.Add(domain.Name, domain);
                            await domainService.SaveAsync(domain);
                            contactWithDomainInfo.Contact.Domain = domain;
                        }
                        else
                        {
                            contactWithDomainInfo.Contact.Domain = existingDomain.FirstOrDefault().Value;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "error");
                throw;
            }
        }

        private void EnrichWithAccountId(List<Contact> contacts)
        {
            foreach (var contact in contacts)
            {
                var domain = contact.Domain;
                if (domain != null)
                {
                    contact.AccountId = domain.AccountId;
                }
            }
        }

        private void EnrichWithAccountId(Contact contact)
        {
            var domain = contact.Domain;
            if (domain != null)
            {
                contact.AccountId = domain.AccountId;
            }
        }

        private async Task SaveIp(Contact contact)
        {
            await contactIpService.SaveAsync(contact);
        }

        private async Task SaveIpRange(List<Contact> contacts)
        {
            await contactIpService.SaveRangeAsync(contacts);
        }
    }
}