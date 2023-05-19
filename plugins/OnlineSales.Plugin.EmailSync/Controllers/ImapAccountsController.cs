// <copyright file="ImapAccountsController.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OnlineSales.Configuration;
using OnlineSales.Data;
using OnlineSales.Exceptions;
using OnlineSales.Plugin.EmailSync.Data;
using OnlineSales.Plugin.EmailSync.DTOs;
using OnlineSales.Plugin.EmailSync.Entities;

namespace OnlineSales.Plugin.EmailSync.Controllers;

[Authorize]
public class ImapAccountsController : ControllerBase
{
    private readonly EmailSyncDbContext dbContext;
    private readonly IMapper mapper;

    public ImapAccountsController(EmailSyncDbContext dbContext, IMapper mapper, IOptions<ApiSettingsConfig> apiSettingsConfig, EsDbContext esDbContext)    
    {
        this.dbContext = dbContext;
        this.mapper = mapper;
    }

    [HttpGet("users/{userid}/imap-accounts/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ImapAccountDetailsDto>> GetAccountForUser(string userid, int id)
    {
        var result = await FindOrThrowNotFound(userid, id);

        var resultConverted = mapper.Map<ImapAccountDetailsDto>(result);

        return Ok(resultConverted);
    }

    [HttpGet("users/{userid}/imap-accounts")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public ActionResult<List<ImapAccountDetailsDto>> GetAccountsForUser(string userid)
    {
        var result = dbContext.ImapAccounts!.Where(a => a.UserId == userid).ToList();
        return Ok(mapper.Map<List<ImapAccountDetailsDto>>(result));
    }

    [HttpPost("users/{userid}/imap-accounts")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ImapAccountDetailsDto>> PostAccountForUser(string userid, [FromBody] ImapAccountCreateDto imapAccount)
    {
        var newValue = mapper.Map<ImapAccount>(imapAccount);
        newValue.UserId = userid;
        var result = await dbContext.ImapAccounts!.AddAsync(newValue);
        await dbContext.SaveChangesAsync();

        var resultsToClient = mapper.Map<ImapAccountDetailsDto>(newValue);

        return CreatedAtAction(nameof(GetAccountsForUser), new { userId = userid, id = result.Entity.Id }, resultsToClient);
    }

    [HttpPatch("users/{userid}/imap-accounts/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public virtual async Task<ActionResult<ImapAccountDetailsDto>> PatchAccountForUser(string userid, int id, [FromBody] ImapAccountUpdateDto value)
    {
        var existingEntity = await FindOrThrowNotFound(userid, id);

        mapper.Map(value, existingEntity);
        await dbContext.SaveChangesAsync();

        var resultsToClient = mapper.Map<ImapAccountDetailsDto>(existingEntity);
        return Ok(resultsToClient);
    }

    [HttpDelete("users/{userid}/imap-accounts/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public virtual async Task<ActionResult> Delete(string userid, int id)
    {
        var existingEntity = await FindOrThrowNotFound(userid, id);

        dbContext.Remove(existingEntity);

        await dbContext.SaveChangesAsync();

        return NoContent();
    }

    private async Task<ImapAccount> FindOrThrowNotFound(string userid, int id)
    {
        var result = await dbContext.ImapAccounts!.Where(a => a.UserId == userid && a.Id == id).FirstOrDefaultAsync();
        if (result == null)
        {
            throw new EntityNotFoundException(typeof(ImapAccount).Name, id.ToString());
        }

        return result;
    }
}
