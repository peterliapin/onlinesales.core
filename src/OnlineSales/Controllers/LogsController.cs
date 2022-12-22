// <copyright file="LogsController.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nest;
using OnlineSales.Entities;
using OnlineSales.ErrorHandling;

namespace OnlineSales.Controllers;

[Authorize]
[Route("api/[controller]")]
public class LogsController : Controller
{
    protected readonly ElasticClient elasticClient;
    protected readonly IErrorMessageGenerator errorMessageGenerator;

    public LogsController(ElasticClient client, IErrorMessageGenerator errorMessageGenerator)
    {
        elasticClient = client;
        this.errorMessageGenerator = errorMessageGenerator;
    }

    // GET api/logs/
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public virtual async Task<ActionResult<List<LogRecord>>> GetAll()
    {
        var logRecords = (
                await elasticClient.SearchAsync<LogRecord>(
                    s => s.Size(20).Skip(10)))
            .Documents.ToList();

        return Ok(logRecords);
    }
}