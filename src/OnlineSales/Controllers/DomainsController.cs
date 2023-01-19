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

namespace OnlineSales.Controllers;

[Authorize]
[Route("api/[controller]")]
public class DomainsController : BaseControllerWithImport<Domain, DomainCreateDto, DomainUpdateDto, DomainDetailsDto, DomainImportDto>
{
    private readonly IDomainVerifyService domainCheckService;

    public DomainsController(ApiDbContext dbContext, IMapper mapper, IOptions<ApiSettingsConfig> apiSettingsConfig, IDomainVerifyService domainCheckService)
        : base(dbContext, mapper, apiSettingsConfig)
    {
        this.domainCheckService = domainCheckService;
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
            domain = new Domain
            {
                Name = name,
            };

            await dbSet.AddAsync(domain);
        }

        await domainCheckService.Verify(domain);
        await dbContext.SaveChangesAsync();

        var resultConverted = mapper.Map<DomainDetailsDto>(domain);

        return Ok(resultConverted);
    }
}

