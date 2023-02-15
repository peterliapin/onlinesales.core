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

        public EmailVerifyService(PgDbContext pgDbContext, IDomainService domainService, IEmailValidationExternalService emailValidationExternalService)
        {
            this.pgContext = pgDbContext;
            this.domainService = domainService;
            this.emailValidationExternalService = emailValidationExternalService;
        }

        public async Task<Domain> VerifyDomain(string email)
        {
            var domainName = email.Split("@").Last();
            
            var domain = await (from d in pgContext.Domains
                                where d.Name == domainName
                                select d).FirstOrDefaultAsync();

            if (domain != null && domain.DnsCheck is true)
            {
                return domain;
            }
            else
            {
                if (domain is null)
                {
                    domain = new Domain()
                    {
                        Name = domainName,
                        Source = email,
                    };

                    pgContext.Add(domain);
                }

                await domainService.Verify(domain!);
                await VerifyEmail(email, domain);
                await pgContext.SaveChangesAsync();

                return domain;
            }
        }

        public async Task VerifyEmail(string email, Domain domain)
        {
            bool freeCheck = false;
            bool disposableCheck = false;
            bool catchAllCheck = false;

            var emailVerify = await emailValidationExternalService.Validate(email);

            if (!bool.TryParse(emailVerify!.FreeCheck, out freeCheck) || !bool.TryParse(emailVerify!.DisposableCheck, out disposableCheck) || !bool.TryParse(emailVerify!.CatchAllCheck, out catchAllCheck))
            {
                throw new KeyNotFoundException("Some values are not found for email validation");
            }

            domain.Free = freeCheck;
            domain.Disposable = disposableCheck;
            domain.CatchAll = catchAllCheck;
        }
    }
}
