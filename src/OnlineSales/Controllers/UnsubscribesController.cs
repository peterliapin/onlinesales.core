// <copyright file="UnsubscribesController.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSales.Data;
using OnlineSales.Entities;

namespace OnlineSales.Controllers;

[Authorize]
[Route("api/[controller]")]
public class UnsubscribesController : Controller
{
    protected readonly PgDbContext dbContext;
    protected readonly IMapper mapper;

    public UnsubscribesController(PgDbContext dbContext, IMapper mapper)
    {
        this.dbContext = dbContext;
        this.mapper = mapper;
    }

    // GET api/unsubscribes/
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public virtual async Task<ActionResult<List<Unsubscribe>>> GetAll()
    {
        var unsibscribes = await (from u in dbContext.Unsubscribes
                                  select u)
                           .ToListAsync();

        return Ok(unsibscribes);
    }
}

