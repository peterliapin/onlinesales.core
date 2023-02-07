// <copyright file="EmailVerifyService.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using Microsoft.EntityFrameworkCore;
using OnlineSales.Data;
using OnlineSales.Entities;
using OnlineSales.Interfaces;

namespace OnlineSales.Services
{
    public class EmailVerifyService : IEmailVerifyService
    {
        private readonly PgDbContext pgContext;
        private readonly IDomainService domainService;
        private readonly IEmailValidationExternalService emailValidationExternalService;

        public EmailVerifyService(PgDbContext apiDbContext, IDomainService domainService, IEmailValidationExternalService emailValidationExternalService)
        {
            this.pgContext = apiDbContext;
            this.domainService = domainService;
            this.emailValidationExternalService = emailValidationExternalService;
        }

        public async Task<Domain> Validate(string email)
        {
            var splittedEmail = email.Split("@");
            var domain = splittedEmail.Last().ToString();
            
            var domainExistance = await GetDomainData(domain);

            if (domainExistance.DomainData != null)
            {
                return domainExistance.DomainData;
            }
            else
            {
                Domain newDomain = new Domain()
                {
                    Name = domain,
                };

                if (!domainExistance.hasDomain)
                {
                    pgContext.Add(newDomain);
                }

                await domainService.Verify(newDomain!);
                await VerifyEmail(email, newDomain);
                await pgContext.SaveChangesAsync();

                return newDomain;
            }
        }

        public async Task<(bool hasDomain, Domain? DomainData)> GetDomainData(string domain)
        {
            bool hasDomain = false;

            var domainData = await (from dbDomain in pgContext.Domains where dbDomain.Name == domain select dbDomain).FirstOrDefaultAsync();

            if (domainData != null)
            {
                hasDomain = true;
            }

            if (domainData != null && domainData!.Free != null && domainData!.Disposable != null && domainData!.DnsRecords != null)
            {
                return (hasDomain, domainData);
            }

            return (hasDomain, null);
        }

        public async Task VerifyEmail(string email, Domain domainRecord)
        {
            bool freeCheck = false;
            bool disposableCheck = false;
            bool catchAllCheck = false;

            var emailVerify = await emailValidationExternalService.Validate(email);

            if (!bool.TryParse(emailVerify!.FreeCheck, out freeCheck) || !bool.TryParse(emailVerify!.DisposableCheck, out disposableCheck) || !bool.TryParse(emailVerify!.CatchAllCheck, out catchAllCheck))
            {
                throw new KeyNotFoundException("Some values are not found for email validation");
            }

            domainRecord.Free = freeCheck;
            domainRecord.Disposable = disposableCheck;
            domainRecord.CatchAll = catchAllCheck;
        }
    }
}
