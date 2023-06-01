// <copyright file="DealsController.cs" company="WavePoint Co. Ltd.">
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
using OnlineSales.Services;

namespace OnlineSales.Controllers;

[Authorize]
[Route("api/[controller]")]
public class DealsController : BaseController<Deal, DealCreateDto, DealUpdateDto, DealDetailsDto>
{
    private readonly IDealService dealService;

    public DealsController(PgDbContext dbContext, IMapper mapper, EsDbContext esDbContext, QueryProviderFactory<Deal> queryProviderFactory, IDealService dealService)
    : base(dbContext, mapper, esDbContext, queryProviderFactory)
    {
        this.dealService = dealService;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public override async Task<ActionResult<DealDetailsDto>> Post([FromBody] DealCreateDto value)
    {
        var newValue = mapper.Map<Deal>(value);

        await dealService.SaveAsync(newValue);
        
        await dbContext.SaveChangesAsync();

        var resultsToClient = mapper.Map<DealDetailsDto>(newValue);

        return CreatedAtAction(nameof(GetOne), new { id = newValue.Id }, resultsToClient);
    }
}