// <copyright file="DomainsController.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Nest;
using OnlineSales.Configuration;
using OnlineSales.Data;
using OnlineSales.DTOs;
using OnlineSales.Entities;

namespace OnlineSales.Controllers;

[Authorize]
[Route("api/[controller]")]
public class DomainsController : BaseController<Domain, DomainCreateDto, DomainUpdateDto, DomainDetailsDto>
{
    public DomainsController(ApiDbContext dbContext, IMapper mapper, IOptions<ApiSettingsConfig> apiSettingsConfig)
        : base(dbContext, mapper, apiSettingsConfig)
    {
    }

    /*
    // GET api/{entity}s/gmail.com
    [HttpGet("{name}")]
    // [EnableQuery]
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
            return new NotFoundResult();
        }
    }*/
}

