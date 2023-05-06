// <copyright file="LogsController.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nest;
using OnlineSales.Data;
using OnlineSales.Entities;

namespace OnlineSales.Controllers;

[Authorize]
[Route("api/[controller]")]
public class LogsController : Controller
{
    protected readonly EsDbContext esDbContext;

    public LogsController(EsDbContext esDbContext)
    {
        this.esDbContext = esDbContext;
    }

    // GET api/logs/
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public virtual async Task<ActionResult<List<LogRecord>>> GetAll()
    {
        var logRecords = (
                await esDbContext.ElasticClient.SearchAsync<LogRecord>(
                    s => s.Size(20).Skip(10)))
            .Documents.ToList();

        return Ok(logRecords);
    }
}