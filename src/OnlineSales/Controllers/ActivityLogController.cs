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
    private readonly PgDbContext dbContext;
    private readonly IOptions<ApiSettingsConfig> apiSettingsConfig;
    private readonly ElasticClient elasticClient;
    private readonly IConfiguration configuration;

    public ActivityLogController(PgDbContext dbContext, IConfiguration configuration, IMapper mapper, IOptions<ApiSettingsConfig> apiSettingsConfig, EsDbContext esDbContext)
    {        
        this.mapper = mapper;
        this.dbContext = dbContext;
        this.apiSettingsConfig = apiSettingsConfig;
        this.configuration = configuration;
        elasticClient = esDbContext.ElasticClient;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public virtual async Task<ActionResult<List<ActivityLogDetailsDto>>> Get([FromQuery] string? query)
    {
        var limit = apiSettingsConfig.Value.MaxListSize;

        var qp = BuildQueryProvider(limit);

        var result = await qp.GetResult();
        Response.Headers.Add(ResponseHeaderNames.TotalCount, result.TotalCount.ToString());
        Response.Headers.Add(ResponseHeaderNames.AccessControlExposeHeader, ResponseHeaderNames.TotalCount);
        return Ok(mapper.Map<List<ActivityLogDetailsDto>>(result.Records));
    }

    private IQueryProvider<ActivityLog> BuildQueryProvider(int maxLimitSize)
    {
        var queryCommands = QueryParser.Parse(Request.QueryString.HasValue ? HttpUtility.UrlDecode(Request.QueryString.ToString()) : string.Empty);
        var queryParams = new QueryData<ActivityLog>(queryCommands, maxLimitSize, dbContext);

        var indexPrefix = configuration.GetSection("Elastic:IndexPrefix").Get<string>();
        return new ESQueryProvider<ActivityLog>(elasticClient, queryParams, indexPrefix!);
    }
}
