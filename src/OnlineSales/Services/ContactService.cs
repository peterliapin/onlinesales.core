// <copyright file="ContactService.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Nest;
using OnlineSales.Data;
using OnlineSales.Entities;
using OnlineSales.Interfaces;

namespace OnlineSales.Services
{
    public class ContactService : IContactService
    {
        private readonly PgDbContext pgDbContext;
        // private readonly IDomainService domainService;
        // private readonly IAccountExternalService accountExternalService;
        // private readonly IEmailVerifyService emailVerifyService;        

        public ContactService(PgDbContext pgDbContext, IDomainService domainService, IAccountExternalService accountExternalService, IEmailVerifyService emailVerifyService)
        {
            this.pgDbContext = pgDbContext;
            // this.domainService = domainService;
            // this.accountExternalService = accountExternalService;
            // this.emailVerifyService = emailVerifyService;
        }

        public async Task SaveAsync(Contact contact)
        {
            await EnrichWithDomainId(contact);
            EnrichWithAccountId(contact);

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

        private async Task EnrichWithDomainId(Contact contact)
        {
            var domainName = GetDomainFromEmail(contact.Email);

            var domainsQueryResult = await pgDbContext!.Domains!.Where(domain => domain.Name == domainName).FirstOrDefaultAsync();

            if (domainsQueryResult != null)
            {
                contact.DomainId = domainsQueryResult.Id;
                contact.Domain = domainsQueryResult;
            }
            else
            {
                contact.Domain = new Domain() { Name = domainName };
            }
        }

        private async Task EnrichWithDomainIdAsync(List<Contact> contacts)
        {
            var newDomains = new Dictionary<string, Domain>();

            var contactsWithDomain = from contact in contacts
                                     select new
                                     {
                                         Contact = contact,
                                         DomainName = GetDomainFromEmail(contact.Email),
                                     };

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
                        };

                        newDomains.Add(domain.Name, domain);
                        await pgDbContext.AddAsync(domain);
                        contactWithDomainInfo.Contact.Domain = domain;
                    }
                    else
                    {
                        contactWithDomainInfo.Contact.Domain = existingDomain.FirstOrDefault().Value;
                    }
                }
            }
        }

        private string GetDomainFromEmail(string email)
        {
            return email.Split("@").Last().ToString();
        }

        private void EnrichWithAccountId(Contact contact)
        {
            if (!contact.AccountId.HasValue)
            {
                Domain domain = contact.Domain!;

                if (domain!.AccountId.HasValue && domain!.AccountId != 0)
                {
                    contact.AccountId = domain!.AccountId;
                }

                // Peter Liapin: 
                // else
                // {
                //     if ((domain.Free == null && domain.Disposable == null) || (domain.Free == false && domain.Disposable == false))
                //     {
                //         await DomainVerifyAndCreateAccount(contact);
                //     }
                // }
            }
        }

        private void EnrichWithAccountId(List<Contact> contacts)
        {
            // var newAccounts = new Dictionary<string, Account>();
            // var updatedContact = new List<Contact>();

            var domainsWithAccounts = (from contact in contacts where contact.Domain!.AccountId.HasValue && contact.Domain!.AccountId != 0 select contact).ToList();

            foreach (var domainWithAccount in domainsWithAccounts)
            {
                domainWithAccount.AccountId = domainWithAccount.Domain!.AccountId;
                domainWithAccount.Account = domainWithAccount.Domain!.Account;

                // updatedContact.Add(domainWithAccount);
            }

            /* foreach (var contact in contacts.Where(contacts => !updatedContact.Any(updatedContact => updatedContact.Email == contacts.Email)))
            {
                Domain domain = contact.Domain!;

                if ((domain.Free == null && domain.Disposable == null) || (domain.Free != true && domain.Disposable != true))
                {
                    var newAccount = await DomainVerifyAndCreateAccount(contact);

                    if (newAccount)
                    {
                        var existingAccount = newAccounts.FirstOrDefault(account => account.Key == contact.Account!.Name);

                        if (existingAccount.Key != default)
                        {
                            contact.Account = existingAccount.Value;
                            contact.Domain!.Account = existingAccount.Value;
                        }
                        else
                        {
                            contact.Domain!.Account = contact.Account;
                            newAccounts.Add(contact.Account!.Name, contact.Account);
                        }
                    }
                }

                updatedContact.Add(contact);
            } */
        }

        // public async Task<bool> DomainVerifyAndCreateAccount(Contact contact)
        // {
        //     bool newAccount = false;
        //     Domain domain = contact.Domain!;
        //     if (domain.Free == null && domain.Disposable == null)
        //     {
        //         await domainService.Verify(domain);
        //         await emailVerifyService.VerifyEmail(contact.Email, domain); 
        //     }
        //     var extAccountInfo = await accountExternalService.GetAccountDetails(domain.Name) !;
        //     var existingDbRecords = (from accounts in pgDbContext.Accounts where accounts.Name == extAccountInfo.Name select accounts).FirstOrDefault();
        //     if (existingDbRecords == null)
        //     {
        //         contact.Account = new Account()
        //         {
        //             Name = extAccountInfo.Name!,
        //             City = extAccountInfo.City,
        //             CountryCode = extAccountInfo.CountryCode,
        //             Revenue = extAccountInfo.Revenue,
        //             EmployeesRange = extAccountInfo.EmployeesRange,
        //             SocialMedia = extAccountInfo.SocialMedia,
        //             State = extAccountInfo.StateCode,
        //             Tags = extAccountInfo.Tags,
        //             Data = extAccountInfo.Data,
        //         };
        //         contact.Domain!.Account = contact.Account;
        //         contact.Domain.AccountSynced = extAccountInfo.AccountSynced;
        //         newAccount = true;
        //     }
        //     else
        //     {
        //         contact.Account = existingDbRecords;
        //         contact.Domain!.Account = contact.Account;
        //     }
        //     return newAccount;
        // }
    }
}
