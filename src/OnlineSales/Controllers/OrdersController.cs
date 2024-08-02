// <copyright file="OrdersController.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineSales.Data;
using OnlineSales.DTOs;
using OnlineSales.Entities;
using OnlineSales.Infrastructure;
using OnlineSales.Interfaces;

namespace OnlineSales.Controllers;

[Authorize(Roles = "Admin")]
[Route("api/[controller]")]
public class OrdersController : BaseControllerWithImport<Order, OrderCreateDto, OrderUpdateDto, OrderDetailsDto, OrderImportDto>
{
    private readonly CommentableControllerExtension commentableControllerExtension;

    public OrdersController(PgDbContext dbContext, IMapper mapper, EsDbContext esDbContext, QueryProviderFactory<Order> queryProviderFactory, CommentableControllerExtension commentableControllerExtension)
        : base(dbContext, mapper, esDbContext, queryProviderFactory)
    {
        this.commentableControllerExtension = commentableControllerExtension;
    }

    [AllowAnonymous]
    [HttpGet("{id}/comments")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<CommentDetailsDto>>> GetComments(int id)
    {
        return commentableControllerExtension.ReturnComments(await commentableControllerExtension.GetCommentsForICommentable<Order>(id), this);
    }

    [AllowAnonymous]
    [HttpPost("{id}/comments")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CommentDetailsDto>> PostComment(int id, [FromBody] CommentCreateBaseDto value)
    {
        return await commentableControllerExtension.PostComment(commentableControllerExtension.CreateCommentForICommentable<Order>(value, id), this);
    }
}