// <copyright file="AccountController.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OnlineSales.Configuration;
using OnlineSales.Data;
using OnlineSales.DTOs;
using OnlineSales.Entities;
using OnlineSales.Interfaces;

namespace OnlineSales.Controllers;

[Authorize]
[Route("api/[controller]")]
public class AccountController : BaseControllerWithImport<Account, AccountCreateDto, AccountUpdateDto, AccountDetailsDto, AccountImportDto>
{
    private readonly IDomainService domainService;

    public AccountController(PgDbContext dbContext, IMapper mapper, IOptions<ApiSettingsConfig> apiSettingsConfig, IDomainService domainService, EsDbContext esDbContext)
        : base(dbContext, mapper, apiSettingsConfig, esDbContext)
    {
        this.domainService = domainService;
    }

    protected override Task BatchWiseSecondaryUpdate(List<Account> batch)
    {
        AddNewDomainsByAccounts(batch);

        return base.BatchWiseSecondaryUpdate(batch);
    }

    private void AddNewDomainsByAccounts(List<Account> batch)
    {
        var importingDomains = batch.Where(a => !string.IsNullOrEmpty(a.SiteUrl)).Select(a => domainService.GetDomainNameByUrl(a.SiteUrl!));

        var uniqueDomains = importingDomains.GroupBy(d => d).Select(g => g.First()).ToList();

        if (uniqueDomains.Any())
        {
            var newDomains = uniqueDomains.Where(d => !dbContext!.Domains!.Select(s => s.Name.ToLower()).Contains(d.ToLower())).ToList();

            if (newDomains.Any())
            {
                foreach (var domain in newDomains)
                {
                    var newDomain = new Domain()
                    {
                        Name = domain,
                        AccountId = batch.Where(b => domainService.GetDomainNameByUrl(b.SiteUrl!) == domain).Select(b => b.Id).First(),
                        CreatedAt = DateTime.UtcNow,
                    };

                    dbContext!.Domains!.Add(newDomain);
                }

                dbContext.SaveChanges();
            }
        }
    }
}
