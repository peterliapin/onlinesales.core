// <copyright file="OrderItemsController.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSales.Data;
using OnlineSales.DTOs;
using OnlineSales.Entities;
using OnlineSales.Infrastructure;
using OnlineSales.Interfaces;

namespace OnlineSales.Controllers;

[Authorize]
[Route("api/[controller]")]
public class OrderItemsController : BaseControllerWithImport<OrderItem, OrderItemCreateDto, OrderItemUpdateDto, OrderItemDetailsDto, OrderItemImportDto>
{
    private readonly IOrderItemService orderItemService;

    public OrderItemsController(PgDbContext dbContext, IMapper mapper, IOrderItemService orderItemService, EsDbContext esDbContext, QueryProviderFactory<OrderItem> queryProviderFactory)
        : base(dbContext, mapper, esDbContext, queryProviderFactory)
    {
        this.orderItemService = orderItemService;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public override async Task<ActionResult<OrderItemDetailsDto>> Post([FromBody] OrderItemCreateDto value)
    {
        var order = await dbContext.Orders!
                            .Include(o => o.OrderItems)
                            .FirstOrDefaultAsync(o => o.Id == value.OrderId);

        if (order == null)
        {
            ModelState.AddModelError("OrderId", "The referenced order was not found");

            throw new InvalidModelStateException(ModelState);
        }

        var orderItem = mapper.Map<OrderItem>(value);
        orderItem.Order = order;

        await orderItemService.SaveAsync(orderItem);

        await dbContext.SaveChangesAsync();

        var returnedValue = mapper.Map<OrderItemDetailsDto>(orderItem);

        return CreatedAtAction(nameof(GetOne), new { id = orderItem.Id }, returnedValue);
    }

    [HttpPatch("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public override async Task<ActionResult<OrderItemDetailsDto>> Patch(int id, [FromBody] OrderItemUpdateDto value)
    {
        var orderItem = await FindOrThrowNotFound(id);

        var order = await dbContext.Orders!
                            .Include(o => o.OrderItems)
                            .FirstOrDefaultAsync(o => o.Id == orderItem.OrderId);

        if (order == null)
        {
            ModelState.AddModelError("OrderId", "The referenced order was not found");

            throw new InvalidModelStateException(ModelState);
        }

        mapper.Map(value, orderItem);

        await orderItemService.SaveAsync(orderItem);

        await dbContext.SaveChangesAsync();

        return mapper.Map<OrderItemDetailsDto>(orderItem);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public override async Task<ActionResult> Delete(int id)
    {
        var orderItem = await FindOrThrowNotFound(id);

        await dbContext.Entry(orderItem)
                .Reference(oi => oi.Order)
                .LoadAsync();

        orderItemService.Delete(orderItem);

        await dbContext.SaveChangesAsync();

        return NoContent();
    }
}