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

        public async Task<Domain> Verify(string email)
        {
            var domainName = this.domainService.GetDomainNameByEmail(email);

            var domain = await (from d in this.pgContext.Domains
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
                    domain = new Domain() { Name = domainName, Source = email };
                    await this.domainService.SaveAsync(domain);
                }

                await this.domainService.Verify(domain!);
                await this.VerifyDomain(email, domain);
                await this.pgContext.SaveChangesAsync();

                return domain;
            }
        }

        public async Task VerifyDomain(string email, Domain domain)
        {
            var emailVerify = await this.emailValidationExternalService.Validate(email);
            domain.CatchAll = emailVerify.CatchAllCheck;
        }
    }
}
