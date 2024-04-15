// <copyright file="UsersController.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSales.Data;
using OnlineSales.DTOs;
using OnlineSales.Entities;
using OnlineSales.Infrastructure;

namespace OnlineSales.Controllers;

[Authorize(Roles = "Admin")]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    protected readonly PgDbContext dbContext;
    protected readonly IMapper mapper;
    protected readonly UserManager<User> userManager;

    public UsersController(PgDbContext dbContext, IMapper mapper, UserManager<User> userManager)
    {
        this.dbContext = dbContext;
        this.mapper = mapper;
        this.userManager = userManager;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserDetailsDto[]>> GetAll()
    {
        var allUsers = await userManager.Users.ToListAsync();
        var resultsToClient = mapper.Map<UserDetailsDto[]>(allUsers).ToArray();
        Response.Headers.Add(ResponseHeaderNames.TotalCount, resultsToClient.Count().ToString());
        Response.Headers.Add(ResponseHeaderNames.AccessControlExposeHeader, ResponseHeaderNames.TotalCount);
        return Ok(resultsToClient);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserDetailsDto>> GetSpecific(string id)
    {
        var existingEntity = await userManager.FindByIdAsync(id);
        if (existingEntity == null)
        {
            throw new EntityNotFoundException(typeof(User).Name, id);
        }

        return Ok(mapper.Map<UserDetailsDto>(existingEntity));
    }

    [HttpPatch("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public virtual async Task<ActionResult<UserDetailsDto>> Patch(string id, [FromBody] UserUpdateDto value)
    {
        var existingEntity = await userManager.FindByIdAsync(id);
        if (existingEntity == null)
        {
            throw new EntityNotFoundException(typeof(User).Name, id);
        }

        mapper.Map(value, existingEntity);
        var result = await userManager.UpdateAsync(existingEntity);
        if (result.Errors.Any())
        {
            throw new IdentityException(result.Errors);
        }

        var resultsToClient = mapper.Map<UserDetailsDto>(existingEntity);

        return Ok(resultsToClient);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public virtual async Task<ActionResult> Delete(string id)
    {
        var existingEntity = await userManager.FindByIdAsync(id);
        if (existingEntity == null)
        {
            throw new EntityNotFoundException(typeof(User).Name, id);
        }

        var result = await userManager.DeleteAsync(existingEntity);
        if (result.Errors.Any())
        {
            throw new IdentityException(result.Errors);
        }

        return NoContent();
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public virtual async Task<ActionResult<UserDetailsDto>> Post([FromBody] UserCreateDto value)
    {
        var newValue = mapper.Map<User>(value);
        var result = await userManager.CreateAsync(newValue);
        if (result.Errors.Any())
        {
            throw new IdentityException(result.Errors);
        }

        var createdUser = await userManager.FindByNameAsync(value.UserName);

        return CreatedAtAction(nameof(GetSpecific), new { id = createdUser!.Id }, createdUser);
    }
}