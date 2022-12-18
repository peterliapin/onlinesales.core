// <copyright file="CommentsController.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nest;
using OnlineSales.Data;
using OnlineSales.DTOs;
using OnlineSales.Entities;

namespace OnlineSales.Controllers;

[Route("api/[controller]")]
public class CommentsController : BaseFKController<Comment, CommentCreateDto, CommentUpdateDto, Post>
{
    public CommentsController(ApiDbContext dbContext, IMapper mapper)
        : base(dbContext, mapper)
    {
    }

    // POST api/{entity}s
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public override async Task<ActionResult<Comment>> Post([FromBody] CommentCreateDto value)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return errorHandler.CreateBadRequestResponce();
            }

            var existFKItem = await (from fk in this.dbFKSet
                                     where fk.Id == GetFKId(value)
                                     select fk).FirstOrDefaultAsync();

            if (existFKItem == null)
            {
                return errorHandler.CreateUnprocessableEntityResponce(CreateNotFoundMessage(GetFKId(value)));
            }

            if (value.ParentId != null)
            {
                var parent = await (from p in this.dbSet
                                            where p.Id == value.ParentId
                                            select p).FirstOrDefaultAsync();

                if (parent == null)
                {
                    return errorHandler.CreateUnprocessableEntityResponce(CreateNotFoundMessage(value.ParentId.Value));
                }
            }

            var newValue = mapper.Map<Comment>(value);
            var result = await dbSet.AddAsync(newValue);
            await dbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetOne), new { id = result.Entity.Id }, value);
        }
        catch (Exception e)
        {
            return errorHandler.CreateInternalServerErrorResponce(e.Message);
        }
    }

    protected override int GetFKId(CommentCreateDto item)
    {
        return item.PostId;
    }

    protected override int? GetFKId(CommentUpdateDto item)
    {
        return null;
    }
}

