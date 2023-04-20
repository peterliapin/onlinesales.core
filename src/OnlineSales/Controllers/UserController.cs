// <copyright file="UserController.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>
using System.Collections.Generic;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Nest;
using OnlineSales.Configuration;
using OnlineSales.Data;
using OnlineSales.DTOs;
using OnlineSales.Entities;
using OnlineSales.Infrastructure;

namespace OnlineSales.Controllers;

[Authorize(AuthenticationSchemes = "Bearer")]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    protected readonly PgDbContext dbContext;
    protected readonly IMapper mapper;
    protected readonly UserManager<User> userManager;
    protected readonly SignInManager<User> signInManager;

    public UserController(PgDbContext dbContext, IMapper mapper, UserManager<User> userManager, SignInManager<User> signInManager)
    {
        this.dbContext = dbContext;
        this.mapper = mapper;
        this.userManager = userManager;
        this.signInManager = signInManager;
    }

    // GET api/user/
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserDetailsDto[]>> GetAll()
    {
        var allUsers = await userManager.Users.ToListAsync();
        var resultsToClient = mapper.Map<UserDetailsDto[]>(allUsers);

        return Ok(resultsToClient);
    }

    // GET api/user/me
    [HttpGet("me")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserDetailsDto>> GetSelf()
    {
        var user = await UserHelper.GetCurrentUserAsync(userManager, this.User);
        return Ok(mapper.Map<UserDetailsDto>(user));
    }

    // GET api/user/5
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

    // PATCH api/user/5
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

    // DELETE api/user/5
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

    // POST api/{entity}s
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
