// <copyright file="DomainsController.cs" company="WavePoint Co. Ltd.">
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
using OnlineSales.Services;

namespace OnlineSales.Controllers;

[Authorize]
[Route("api/[controller]")]
public class DomainsController : BaseControllerWithImport<Domain, DomainCreateDto, DomainUpdateDto, DomainDetailsDto, DomainImportDto>
{
    private readonly IDomainService domainService;

    public DomainsController(PgDbContext dbContext, IMapper mapper, IOptions<ApiSettingsConfig> apiSettingsConfig, IDomainService domainService, EsDbContext esDbContext)
        : base(dbContext, mapper, apiSettingsConfig, esDbContext)
    {
        this.domainService = domainService;
    }

    // GET api/domains/names/gmail.com
    [HttpGet("verify/{name}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<DomainDetailsDto>> Verify(string name)
    {
        var domain = (from d in this.dbSet
                      where d.Name == name
                      select d).FirstOrDefault();

        if (domain == null)
        {
            domain = new Domain() { Name = name };
            await this.domainService.SaveAsync(domain);
        }

        await this.domainService.Verify(domain);
        await this.dbContext.SaveChangesAsync();

        var resultConverted = this.mapper.Map<DomainDetailsDto>(domain);

        return this.Ok(resultConverted);
    }

    protected override async Task SaveRangeAsync(List<Domain> newRecords)
    {
        await this.domainService.SaveRangeAsync(newRecords);
    }
}

