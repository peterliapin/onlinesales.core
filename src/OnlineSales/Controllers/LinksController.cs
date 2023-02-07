// <copyright file="LinksController.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OnlineSales.Configuration;
using OnlineSales.Data;
using OnlineSales.DTOs;
using OnlineSales.Entities;
using OnlineSales.Helpers;

namespace OnlineSales.Controllers;

[Authorize]
[Route("api/[controller]")]
public class LinksController : BaseController<Link, LinkCreateDto, LinkUpdateDto, LinkDetailsDto>
{
    public LinksController(PgDbContext dbContext, IMapper mapper, IOptions<ApiSettingsConfig> apiSettingsConfig)
        : base(dbContext, mapper, apiSettingsConfig)
    {
    }

    // POST api/links
    [HttpPost]
    public override Task<ActionResult<LinkDetailsDto>> Post([FromBody] LinkCreateDto link)
    {
        if (string.IsNullOrEmpty(link.Uid))
        {
            link.Uid = UidHelper.Generate();
        }

        return base.Post(link);
    }

    [HttpGet]
    [AllowAnonymous]
    [Route("/go/{uid}")]
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

