// <copyright file="LogsController.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nest;
using OnlineSales.Entities;

namespace OnlineSales.Controllers;

[Route("api/[controller]")]
public class LogsController : Controller
{
    protected readonly ElasticClient elasticClient;

    public LogsController(ElasticClient client)
    {
        elasticClient = client;
    }

    // GET api/logs/
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public virtual async Task<ActionResult<List<LogRecord>>> GetAll()
    {
        try
        {
            var logRecords = (await elasticClient.SearchAsync<LogRecord>())
                .Documents.ToList();

            return Ok(logRecords);
        }
        catch (Exception ex)
        {
            return Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                title: ex.Message,
                detail: ex.StackTrace);
        }
    }
}