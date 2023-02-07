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
        private readonly PgDbContext apiDbContext;
        private readonly IDomainService domainService;
        private readonly IAccountExternalService accountExternalService;
        private readonly IEmailVerifyService emailVerifyService;        

        public ContactService(PgDbContext apiDbContext, IDomainService domainService, IAccountExternalService accountExternalService, IEmailVerifyService emailVerifyService)
        {
            this.apiDbContext = apiDbContext;
            this.domainService = domainService;
            this.accountExternalService = accountExternalService;
            this.emailVerifyService = emailVerifyService;
        }

        public async Task<Contact> AddContact(Contact contact)
        {
            var domainReturnedValue = EnrichWithDomainId(contact);

            var accountReturnedValue = await EnrichWithAccountId(domainReturnedValue);

            await apiDbContext.Contacts!.AddAsync(accountReturnedValue);

            await apiDbContext.SaveChangesAsync();

            return accountReturnedValue!;
        }

        public async Task<Contact> UpdateContact(Contact contact)
        {
            var domainReturnedValue = EnrichWithDomainId(contact);

            var accountReturnedValue = await EnrichWithAccountId(domainReturnedValue);

            apiDbContext.Contacts!.Update(accountReturnedValue);

            await apiDbContext.SaveChangesAsync();

            return accountReturnedValue;
        }

        public Contact EnrichWithDomainId(Contact contact)
        {
            var domainName = GetDomainFromEmail(contact.Email);

            var domainsQueryResult = apiDbContext!.Domains!.Where(domain => domain.Name == domainName).FirstOrDefault();

            if (domainsQueryResult != null)
            {
                contact.DomainId = domainsQueryResult.Id;
                contact.Domain = domainsQueryResult;
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
                                      select new { EnteredContact = contactWithDomain.Contact, DomainName = contactWithDomain.DomainName, DbDomain = domain, DomainId = domain?.Id ?? 0 }).ToList();

            foreach (var domainItem in domainsQueryResult)
            {
                if (domainItem.DomainId != 0)
                {
                    domainItem.EnteredContact.DomainId = domainItem.DomainId;
                    domainItem.EnteredContact.Domain = domainItem.DbDomain;
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

        public async Task<Contact> EnrichWithAccountId(Contact contact)
        {
            Domain domain = contact.Domain!;

            if (domain!.AccountId.HasValue && domain!.AccountId != 0)
            {
                contact.AccountId = domain!.AccountId;
                return contact;
            }
            else
            {
                if ((domain.Free == null && domain.Disposable == null) || (domain.Free != true && domain.Disposable != true))
                {
                    var newContact = (await DomainVerifyAndCreateAccount(contact)).contact!;

                    return newContact;
                }
                else
                {
                    return contact;
                }
            }
        }

        public List<Contact> EnrichWithAccountId(List<Contact> contacts)
        {
            Dictionary<string, Account> newAccounts = new Dictionary<string, Account>();
            List<Contact> updatedContact = new List<Contact>();

            var domainsWithAccounts = (from contact in contacts where contact.Domain!.AccountId.HasValue && contact.Domain!.AccountId != 0 select contact).ToList();

            foreach (var domainWithAccount in domainsWithAccounts)
            {
                domainWithAccount.AccountId = domainWithAccount.Domain!.AccountId;
                domainWithAccount.Account = domainWithAccount.Domain!.Account;

                updatedContact.Add(domainWithAccount);
            }

            foreach (var contact in contacts.Where(contacts => !updatedContact.Any(updatedContact => updatedContact.Email == contacts.Email)))
            {
                Domain domain = contact.Domain!;

                if ((domain.Free == null && domain.Disposable == null) || (domain.Free != true && domain.Disposable != true))
                {
                    var newContact = DomainVerifyAndCreateAccount(contact).Result !;

                    if (newContact.newAccount)
                    {
                        var existingAccount = newAccounts.FirstOrDefault(account => account.Key == newContact.contact.Account!.Name);

                        if (existingAccount.Key != default)
                        {
                            contact.Account = existingAccount.Value;
                            contact.Domain!.Account = existingAccount.Value;
                        }
                        else
                        {
                            contact.Account = newContact.contact.Account;
                            contact.Domain!.Account = newContact.contact.Account;
                            newAccounts.Add(newContact.contact.Account!.Name, newContact.contact.Account);
                        }
                    }
                }

                updatedContact.Add(contact);
            }

            return updatedContact;
        }

        public async Task<(bool newAccount, Contact contact)> DomainVerifyAndCreateAccount(Contact contact)
        {
            bool newAccount = false;
            Domain domain = contact.Domain!;

            if (domain.Free == null && domain.Disposable == null)
            {
                await domainService.Verify(domain);
                await emailVerifyService.VerifyEmail(contact.Email, domain); 
            }

            var extAccountInfo = await accountExternalService.GetAccountDetails(domain.Name) !;

            var existingDbRecords = (from accounts in apiDbContext.Accounts where accounts.Name == extAccountInfo.Name select accounts).FirstOrDefault();

            if (existingDbRecords == null)
            {
                contact.Account = new Account()
                {
                    Name = extAccountInfo.Name!,
                    City = extAccountInfo.City,
                    CountryCode = extAccountInfo.CountryCode,
                    Revenue = extAccountInfo.Revenue,
                    EmployeesRange = extAccountInfo.EmployeesRange,
                    SocialMedia = extAccountInfo.SocialMedia,
                    StateCode = extAccountInfo.StateCode,
                    Tags = extAccountInfo.Tags,
                    Data = extAccountInfo.Data,
                };

                contact.Domain!.Account = contact.Account;
                contact.Domain.AccountSynced = extAccountInfo.AccountSynced;

                newAccount = true;
            }
            else
            {
                contact.Account = existingDbRecords;
                contact.Domain!.Account = contact.Account;
            }

            return (newAccount, contact);
        }
    }
}
