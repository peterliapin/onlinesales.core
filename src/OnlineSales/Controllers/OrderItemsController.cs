// <copyright file="OrderItemsController.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OnlineSales.Configuration;
using OnlineSales.Data;
using OnlineSales.DTOs;
using OnlineSales.Entities;
using OnlineSales.Interfaces;

namespace OnlineSales.Controllers;

[Authorize]
[Route("api/[controller]")]
public class OrderItemsController : BaseControllerWithImport<OrderItem, OrderItemCreateDto, OrderItemUpdateDto, OrderItemDetailsDto, OrderItemImportDto>
{
    private readonly IOrderItemService orderItemService;

    public OrderItemsController(PgDbContext dbContext, IMapper mapper, IOrderItemService orderItemService, IOptions<ApiSettingsConfig> apiSettingsConfig, EsDbContext esDbContext)
        : base(dbContext, mapper, apiSettingsConfig, esDbContext)
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
        var existOrder = await (from order in this.dbContext.Orders
                                    where order.Id == value.OrderId
                                    select order).FirstOrDefaultAsync();

        if (existOrder == null)
        {
            ModelState.AddModelError("OrderId", "The referenced order was not found");

            throw new InvalidModelStateException(ModelState);
        }

        var orderItem = mapper.Map<OrderItem>(value);

        var createdItemId = await orderItemService.AddOrderItem(existOrder, orderItem);

        var returnedValue = mapper.Map<OrderItemDetailsDto>(orderItem);

        return CreatedAtAction(nameof(GetOne), new { id = createdItemId }, returnedValue);
    }

    [HttpPatch("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public override async Task<ActionResult<OrderItemDetailsDto>> Patch(int id, [FromBody] OrderItemUpdateDto value)
    {
        var existingEntity = await FindOrThrowNotFound(id);

        var existingOrder = await (from order in this.dbContext.Orders
                                where order.Id == existingEntity.OrderId
                                select order).FirstOrDefaultAsync();

        mapper.Map(value, existingEntity);

        var updatedItem = await orderItemService.UpdateOrderItem(existingOrder!, existingEntity);

        var returnItem = mapper.Map<OrderItemDetailsDto>(updatedItem);

        return returnItem;
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public override async Task<ActionResult> Delete(int id)
    {
        var existingEntity = await FindOrThrowNotFound(id);

        var existingOrder = await (from order in this.dbContext.Orders
                                   where order.Id == existingEntity.OrderId
                                   select order).FirstOrDefaultAsync();

        await orderItemService.DeleteOrderItem(existingOrder!, existingEntity);

        return NoContent();
    }
}