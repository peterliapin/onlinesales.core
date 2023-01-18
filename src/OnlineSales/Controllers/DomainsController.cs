// <copyright file="DomainsController.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Xml.Linq;
using AutoMapper;
using DnsClient;
using Elasticsearch.Net;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using OnlineSales.Configuration;
using OnlineSales.Data;
using OnlineSales.DTOs;
using OnlineSales.Entities;
using OnlineSales.Helpers;
using OnlineSales.Interfaces;

namespace OnlineSales.Controllers;

[Authorize]
[Route("api/[controller]")]
public class DomainsController : BaseControllerWithImport<Domain, DomainCreateDto, DomainUpdateDto, DomainDetailsDto, DomainImportDto>
{
    private readonly IDomainCheckService domainCheckService;

    public DomainsController(ApiDbContext dbContext, IMapper mapper, IOptions<ApiSettingsConfig> apiSettingsConfig, IDomainCheckService domainCheckService)
        : base(dbContext, mapper, apiSettingsConfig)
    {
        this.domainCheckService = domainCheckService;
    }

    // GET api/domains/names/gmail.com
    [HttpGet("names/{name}")]    
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<DomainDetailsDto>> GetOne(string name)
    {
        var existingEntity = from d in this.dbSet where d.Name == name select d;

        if (existingEntity != null && existingEntity.Any())
        {
            var domain = await existingEntity.FirstAsync();
            return mapper.Map<DomainDetailsDto>(domain);
        }
        else
        {
            var domain = new Domain();
            domain.Name = name;

            await domainCheckService.HttpCheck(domain);
            await domainCheckService.DnsCheck(domain);

            var result = await dbSet.AddAsync(domain);
            await dbContext.SaveChangesAsync();

            return await GetOne(result.Entity.Id);
        }
    }
}

