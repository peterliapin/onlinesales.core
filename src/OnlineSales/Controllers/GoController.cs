// <copyright file="GoController.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSales.Data;
using OnlineSales.Entities;
using Swashbuckle.AspNetCore.Annotations;

namespace OnlineSales.Controllers;

public class GoController : ControllerBase
{
    private readonly PgDbContext dbContext;

    public GoController(PgDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    [HttpGet]
    [Route("/go/{uid}")]
    [AllowAnonymous]
    [SwaggerOperation(Tags = new[] { "Links" })]
    public async Task<ActionResult> Follow(string uid)
    {
        var link = await (from l in dbContext.Links
                          where l.Uid == uid
                          select l).FirstOrDefaultAsync();

        if (link == null)
        {
            throw new EntityNotFoundException(typeof(Link).Name, uid.ToString());
        }

        var linkLog = new LinkLog
        {
            Destination = link.Destination,
            Referrer = Request.Headers.Referer.ToString(),
            LinkId = link.Id,
        };

        await dbContext.LinkLogs!.AddAsync(linkLog);
        await dbContext.SaveChangesAsync();

        return new RedirectResult(linkLog.Destination, false, true);
    }
}

