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

[Authorize]
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

    // GET api/user/
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserDetailsDto[]>> GetAll()
    {
        var allUsers = await this.userManager.Users.ToListAsync();
        var resultsToClient = this.mapper.Map<UserDetailsDto[]>(allUsers).ToArray();
        this.Response.Headers.Add(ResponseHeaderNames.TotalCount, resultsToClient.Count().ToString());
        this.Response.Headers.Add(ResponseHeaderNames.AccessControlExposeHeader, ResponseHeaderNames.TotalCount);
        return this.Ok(resultsToClient);
    }

    // GET api/user/me
    [HttpGet("me")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserDetailsDto>> GetSelf()
    {
        var user = await UserHelper.GetCurrentUserAsync(this.userManager, this.User);
        return this.Ok(this.mapper.Map<UserDetailsDto>(user));
    }

    // GET api/user/5
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserDetailsDto>> GetSpecific(string id)
    {
        var existingEntity = await this.userManager.FindByIdAsync(id);
        if (existingEntity == null)
        {
            throw new EntityNotFoundException(typeof(User).Name, id);
        }

        return this.Ok(this.mapper.Map<UserDetailsDto>(existingEntity));
    }

    // PATCH api/user/5
    [HttpPatch("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public virtual async Task<ActionResult<UserDetailsDto>> Patch(string id, [FromBody] UserUpdateDto value)
    {
        var existingEntity = await this.userManager.FindByIdAsync(id);
        if (existingEntity == null)
        {
            throw new EntityNotFoundException(typeof(User).Name, id);
        }

        this.mapper.Map(value, existingEntity);
        var result = await this.userManager.UpdateAsync(existingEntity);
        if (result.Errors.Any())
        {
            throw new IdentityException(result.Errors);
        }

        var resultsToClient = this.mapper.Map<UserDetailsDto>(existingEntity);

        return this.Ok(resultsToClient);
    }

    // DELETE api/user/5
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public virtual async Task<ActionResult> Delete(string id)
    {
        var existingEntity = await this.userManager.FindByIdAsync(id);
        if (existingEntity == null)
        {
            throw new EntityNotFoundException(typeof(User).Name, id);
        }

        var result = await this.userManager.DeleteAsync(existingEntity);
        if (result.Errors.Any())
        {
            throw new IdentityException(result.Errors);
        }

        return this.NoContent();
    }

    // POST api/{entity}s
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public virtual async Task<ActionResult<UserDetailsDto>> Post([FromBody] UserCreateDto value)
    {
        var newValue = this.mapper.Map<User>(value);
        var result = await this.userManager.CreateAsync(newValue);
        if (result.Errors.Any())
        {
            throw new IdentityException(result.Errors);
        }

        var createdUser = await this.userManager.FindByNameAsync(value.UserName);

        return this.CreatedAtAction(nameof(GetSpecific), new { id = createdUser!.Id }, createdUser);
    }
}
