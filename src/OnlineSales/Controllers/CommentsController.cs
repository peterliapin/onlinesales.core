// <copyright file="CommentsController.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSales.Data;
using OnlineSales.DTOs;
using OnlineSales.Entities;

namespace OnlineSales.Controllers;

[Authorize]
[Route("api/[controller]")]
public class CommentsController : BaseFKController<Comment, CommentCreateDto, CommentUpdateDto, Post>
{
    public CommentsController(ApiDbContext dbContext, IMapper mapper)
        : base(dbContext, mapper)
    {
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public override Task<ActionResult<List<Comment>>> Get([FromQuery] IDictionary<string, string>? parameters)
    {
        return base.Get(parameters);
    }

    // GET api/{entity}s/5
    [HttpGet("{id}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public override Task<ActionResult<Comment>> GetOne(int id)
    {
        return base.GetOne(id);
    }

    // POST api/{entity}s
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public override async Task<ActionResult<Comment>> Post([FromBody] CommentCreateDto value)
    {
        var existFKItem = await (from fk in this.dbFKSet
                                    where fk.Id == GetFKId(value).Item1
                                    select fk).FirstOrDefaultAsync();

        if (existFKItem == null)
        {
            ModelState.AddModelError(GetFKId(value).Item2, "The referenced object was not found");

            throw new InvalidModelStateException(ModelState);
        }

        if (value.ParentId != null)
        {
            var parent = await (from p in this.dbSet
                                where p.Id == value.ParentId
                                select p).FirstOrDefaultAsync();

            if (parent == null)
            {
                ModelState.AddModelError("ParentId", "The referenced object was not found");

                throw new InvalidModelStateException(ModelState);
            }
        }

        var newValue = mapper.Map<Comment>(value);
        var result = await dbSet.AddAsync(newValue);
        await dbContext.SaveChangesAsync();

        return CreatedAtAction(nameof(GetOne), new { id = result.Entity.Id }, value);
    }

    protected override (int, string) GetFKId(CommentCreateDto item)
    {
        return (item.PostId, "PostId");
    }

    protected override (int?, string) GetFKId(CommentUpdateDto item)
    {
        return (null, string.Empty);
    }
}