// <copyright file="ImapAccountsController.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSales.Exceptions;
using OnlineSales.Plugin.EmailSync.Data;
using OnlineSales.Plugin.EmailSync.DTOs;
using OnlineSales.Plugin.EmailSync.Entities;
using Swashbuckle.AspNetCore.Annotations;

namespace OnlineSales.Plugin.EmailSync.Controllers;

[Authorize(Roles = "Admin")]
[Route("api/users")]
public class ImapAccountsController : ControllerBase
{
    private readonly EmailSyncDbContext dbContext;
    private readonly IMapper mapper;

    public ImapAccountsController(EmailSyncDbContext dbContext, IMapper mapper)
    {
        this.dbContext = dbContext;
        this.mapper = mapper;
    }

    [SwaggerOperation(Tags = new[] { "Users" })]
    [HttpGet("{userId}/imap-accounts/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]    
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ImapAccountDetailsDto>> GetAccountForUser(string userId, int id)
    {
        var result = await FindOrThrowNotFound(userId, id);

        var resultConverted = mapper.Map<ImapAccountDetailsDto>(result);

        return Ok(resultConverted);
    }

    [SwaggerOperation(Tags = new[] { "Users" })]
    [HttpGet("{userId}/imap-accounts")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public ActionResult<List<ImapAccountDetailsDto>> GetAccountsForUser(string userId)
    {
        var result = dbContext.ImapAccounts!.Where(a => a.UserId == userId).ToList();
        return Ok(mapper.Map<List<ImapAccountDetailsDto>>(result));
    }

    [SwaggerOperation(Tags = new[] { "Users" })]
    [HttpPost("{userId}/imap-accounts")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ImapAccountDetailsDto>> PostAccountForUser(string userId, [FromBody] ImapAccountCreateDto imapAccount)
    {
        var newValue = mapper.Map<ImapAccount>(imapAccount);
        newValue.UserId = userId;
        var result = await dbContext.ImapAccounts!.AddAsync(newValue);
        await dbContext.SaveChangesAsync();

        var resultsToClient = mapper.Map<ImapAccountDetailsDto>(newValue);

        return CreatedAtAction(nameof(GetAccountsForUser), new { userId = userId, id = result.Entity.Id }, resultsToClient);
    }

    [SwaggerOperation(Tags = new[] { "Users" })]
    [HttpPatch("{userId}/imap-accounts/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public virtual async Task<ActionResult<ImapAccountDetailsDto>> PatchAccountForUser(string userId, int id, [FromBody] ImapAccountUpdateDto value)
    {
        var existingEntity = await FindOrThrowNotFound(userId, id);

        mapper.Map(value, existingEntity);
        await dbContext.SaveChangesAsync();

        var resultsToClient = mapper.Map<ImapAccountDetailsDto>(existingEntity);
        return Ok(resultsToClient);
    }

    [SwaggerOperation(Tags = new[] { "Users" })]
    [HttpDelete("{userId}/imap-accounts/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public virtual async Task<ActionResult> Delete(string userId, int id)
    {
        var existingEntity = await FindOrThrowNotFound(userId, id);

        dbContext.Remove(existingEntity);

        await dbContext.SaveChangesAsync();

        return NoContent();
    }

    private async Task<ImapAccount> FindOrThrowNotFound(string userId, int id)
    {
        var result = await dbContext.ImapAccounts!.Where(a => a.UserId == userId && a.Id == id).FirstOrDefaultAsync();
        if (result == null)
        {
            throw new EntityNotFoundException(typeof(ImapAccount).Name, id.ToString());
        }

        return result;
    }
}
