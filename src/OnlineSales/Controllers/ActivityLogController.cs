// <copyright file="ActivityLogController.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Web;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Nest;
using OnlineSales.Configuration;
using OnlineSales.Data;
using OnlineSales.DTOs;
using OnlineSales.Entities;
using OnlineSales.Infrastructure;

namespace OnlineSales.Controllers;

[Authorize]
[Route("api/[controller]")]
public class ActivityLogController : ControllerBase
{    
    private readonly IMapper mapper;
    private readonly QueryProviderFactory<ActivityLog> queryFactory;

    public ActivityLogController(PgDbContext dbContext, IConfiguration configuration, IMapper mapper, IOptions<ApiSettingsConfig> apiSettingsConfig, EsDbContext esDbContext, QueryProviderFactory<ActivityLog> queryFactory)
    {        
        this.mapper = mapper;
        this.queryFactory = queryFactory;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public virtual async Task<ActionResult<List<ActivityLogDetailsDto>>> Get([FromQuery] string? query)
    {
        var qp = queryFactory.BuildQueryProvider();

        var result = await qp.GetResult();
        Response.Headers.Add(ResponseHeaderNames.TotalCount, result.TotalCount.ToString());
        Response.Headers.Add(ResponseHeaderNames.AccessControlExposeHeader, ResponseHeaderNames.TotalCount);
        return Ok(mapper.Map<List<ActivityLogDetailsDto>>(result.Records));
    }
}
