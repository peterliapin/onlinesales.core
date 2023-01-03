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

namespace OnlineSales.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class OrderItemsController : BaseFKController<OrderItem, OrderItemCreateDto, OrderItemUpdateDto, Order, OrderItemDetailsDto>
    {
        private readonly IOrderItemService orderItemService;

        public OrderItemsController(ApiDbContext dbContext, IMapper mapper, IOrderItemService orderItemService, IOptions<ApiSettingsConfig> apiSettingsConfig)
            : base(dbContext, mapper, apiSettingsConfig)
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
            var existFKItem = await (from fk in this.dbFKSet
                                        where fk.Id == GetFKId(value).Item1
                                        select fk).FirstOrDefaultAsync();

            if (existFKItem == null)
            {
                ModelState.AddModelError(GetFKId(value).Item2, "The referenced object was not found");

                throw new InvalidModelStateException(ModelState);
            }

            var orderItem = mapper.Map<OrderItem>(value);

            var createdItemId = await orderItemService.AddOrderItem(existFKItem, orderItem);

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

            var existFKItem = await (from fk in this.dbFKSet
                                     where fk.Id == existingEntity.OrderId
                                     select fk).FirstOrDefaultAsync();

            if (existFKItem == null)
            {
                ModelState.AddModelError("OrderId", "The referenced object was not found");

                throw new InvalidModelStateException(ModelState);
            }

            mapper.Map(value, existingEntity);

            var updatedItem = await orderItemService.UpdateOrderItem(existFKItem, existingEntity);

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

            var existFKItem = await (from fk in this.dbFKSet
                                        where fk.Id == existingEntity.OrderId
                                        select fk).FirstOrDefaultAsync();

            if (existFKItem == null)
            {
                ModelState.AddModelError("OrderId", "The referenced object was not found");

                throw new InvalidModelStateException(ModelState);
            }

            await orderItemService.DeleteOrderItem(existFKItem, existingEntity);

            return NoContent();
        }

        protected override (int, string) GetFKId(OrderItemCreateDto item)
        {
            return (item.OrderId, "OrderId");
        }

        protected override (int?, string) GetFKId(OrderItemUpdateDto item)
        {
            return (null, string.Empty);
        }
    }
}