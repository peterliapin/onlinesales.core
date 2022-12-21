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
using OnlineSales.ErrorHandling;
using OnlineSales.Interfaces;

namespace OnlineSales.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class OrderItemsController : BaseFKController<OrderItem, OrderItemCreateDto, OrderItemUpdateDto, Order>
    {
        private readonly IOrderItemService orderItemService;

        public OrderItemsController(ApiDbContext dbContext, IMapper mapper, IOrderItemService orderItemService, IErrorMessageGenerator errorMessageGenerator)
            : base(dbContext, mapper, errorMessageGenerator)
        {
            this.orderItemService = orderItemService;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public override async Task<ActionResult<OrderItem>> Post([FromBody] OrderItemCreateDto value)
        {
            if (!ModelState.IsValid)
            {
                return CreateValidationErrorMessageResult();
            }

            var existFKItem = await (from fk in this.dbFKSet
                                        where fk.Id == GetFKId(value)
                                        select fk).FirstOrDefaultAsync();

            if (existFKItem == null)
            {
                return CreateUnprocessableEntityResult(GetFKId(value));
            }

            var orderItem = mapper.Map<OrderItem>(value);

            var credtedItem = await orderItemService.AddOrderItem(existFKItem, orderItem);
            return CreatedAtAction(nameof(GetOne), new { id = credtedItem }, value);
        }

        [HttpPatch("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public override async Task<ActionResult<OrderItem>> Patch(int id, [FromBody] OrderItemUpdateDto value)
        {
            if (!ModelState.IsValid)
            {
                return CreateValidationErrorMessageResult();
            }

            var existingEntity = await (from p in this.dbSet
                                        where p.Id == id
                                        select p).FirstOrDefaultAsync();

            if (existingEntity == null)
            {
                return CreateNotFoundMessageResult(id);
            }

            var existFKItem = await (from fk in this.dbFKSet
                                        where fk.Id == existingEntity.OrderId
                                        select fk).FirstOrDefaultAsync();

            if (existFKItem == null)
            {
                return CreateUnprocessableEntityResult(existingEntity.OrderId);
            }

            mapper.Map(value, existingEntity);

            var updatedItem = await orderItemService.UpdateOrderItem(existFKItem, existingEntity);

            return updatedItem;
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public override async Task<ActionResult> Delete(int id)
        {
            var existingEntity = await (from p in this.dbSet
                                        where p.Id == id
                                        select p).FirstOrDefaultAsync();

            if (existingEntity == null)
            {
                return CreateNotFoundMessageResult(id);
            }

            var existFKItem = await (from fk in this.dbFKSet
                                        where fk.Id == existingEntity.OrderId
                                        select fk).FirstOrDefaultAsync();

            if (existFKItem == null)
            {
                return CreateUnprocessableEntityResult(existingEntity.OrderId);
            }

            await orderItemService.DeleteOrderItem(existFKItem, existingEntity);

            return NoContent();
        }

        protected override int GetFKId(OrderItemCreateDto item)
        {
            return item.OrderId;
        }

        protected override int? GetFKId(OrderItemUpdateDto item)
        {
            return null;
        }
    }
}


