// <copyright file="MeController.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OnlineSales.DTOs;
using OnlineSales.Entities;
using OnlineSales.Helpers;
using Swashbuckle.AspNetCore.Annotations;

namespace OnlineSales.Controllers;

[Authorize]
[Route("api/users")]
public class MeController : ControllerBase
{
    private readonly IMapper mapper;
    private readonly UserManager<User> userManager;

    public MeController(IMapper mapper, UserManager<User> userManager)
    {
        this.mapper = mapper;
        this.userManager = userManager;
    }
    
    [HttpGet("me")]
    [SwaggerOperation(Tags = new[] { "Users" })]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserDetailsDto>> GetSelf()
    {
        var user = await UserHelper.GetCurrentUserOrThrowAsync(userManager, User);

        return Ok(mapper.Map<UserDetailsDto>(user));
    }

    [HttpPatch("me")]
    [SwaggerOperation(Tags = new[] { "Users" })]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public virtual async Task<ActionResult<UserDetailsDto>> Patch([FromBody] UserUpdateDto value)
    {
        var user = await UserHelper.GetCurrentUserOrThrowAsync(userManager, User);
            
        mapper.Map(value, user);

        await userManager.UpdateAsync(user);

        var userDetails = mapper.Map<UserDetailsDto>(user);

        return Ok(userDetails);
    }
}

