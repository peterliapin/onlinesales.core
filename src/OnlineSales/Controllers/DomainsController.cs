// <copyright file="DomainsController.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineSales.Data;
using OnlineSales.DTOs;
using OnlineSales.Entities;
using OnlineSales.Infrastructure;
using OnlineSales.Interfaces;

namespace OnlineSales.Controllers;

[Authorize(Roles = "Admin")]
[Route("api/[controller]")]
public class DomainsController : BaseControllerWithImport<Domain, DomainCreateDto, DomainUpdateDto, DomainDetailsDto, DomainImportDto>
{
    private readonly IDomainService domainService;

    public DomainsController(PgDbContext dbContext, IMapper mapper, IDomainService domainService, EsDbContext esDbContext, QueryProviderFactory<Domain> queryProviderFactory)
        : base(dbContext, mapper, esDbContext, queryProviderFactory)
    {
        this.domainService = domainService;
    }

    // GET api/domains/names/gmail.com
    [HttpGet("verify/{name}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<DomainDetailsDto>> Verify(string name, bool force = false)
    {
        var domain = (from d in dbSet
                      where d.Name == name
                      select d).FirstOrDefault();

        if (domain == null)
        {
            domain = new Domain() { Name = name };
            await domainService.SaveAsync(domain);
        }

        if (force)
        {
            domain.Title = null;
            domain.Description = null;
            domain.DnsRecords = null;
            domain.DnsCheck = null;
            domain.HttpCheck = null;
            domain.MxCheck = null;
            domain.Url = null;
        }

        await domainService.Verify(domain);
        await dbContext.SaveChangesAsync();

        var resultConverted = mapper.Map<DomainDetailsDto>(domain);

        return Ok(resultConverted);
    }

    protected override async Task SaveRangeAsync(List<Domain> newRecords)
    {
        await domainService.SaveRangeAsync(newRecords);
    }
}