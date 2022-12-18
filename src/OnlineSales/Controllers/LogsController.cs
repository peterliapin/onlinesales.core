// <copyright file="LogsController.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using Microsoft.AspNetCore.Mvc;
using Nest;
using OnlineSales.Entities;

namespace OnlineSales.Controllers;

[Route("api/[controller]")]
public class LogsController : ControllerEH
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
            var logRecords = (
                    await elasticClient.SearchAsync<LogRecord>(
                        s => s.Size(20).Skip(10)))
                .Documents.ToList();

            return Ok(logRecords);
        }
        catch (Exception ex)
        {
            return errorHandler.CreateInternalServerErrorResponce(ex.Message);
        }
    }
}