// <copyright file="OrderItemsController.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSales.Data;
using OnlineSales.DTOs;
using OnlineSales.Entities;
using OnlineSales.Interfaces;

namespace OnlineSales.Controllers
{
    [Route("api/[controller]")]
    public class OrderItemsController : BaseFKController<OrderItem, OrderItemCreateDto, OrderItemUpdateDto, Order>
    {
        private readonly IOrderItemService orderItemService;

        public OrderItemsController(ApiDbContext dbContext, IMapper mapper, IOrderItemService orderItemService)
            : base(dbContext, mapper)
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
            try
            {
                if (!ModelState.IsValid)
                {
                    return UnprocessableEntity(ModelState);
                }

                var existFKItem = await (from fk in this.dbFKSet
                                         where fk.Id == GetFKId(value)
                                         select fk).FirstOrDefaultAsync();

                if (existFKItem == null)
                {
                    return errorHandler.CreateUnprocessableEntityResponce(CreateNotFoundMessage<Order>(GetFKId(value)));
                }

                var orderItem = mapper.Map<OrderItem>(value);

                var credtedItem = await orderItemService.AddOrderItem(existFKItem, orderItem);
                return CreatedAtAction(nameof(GetOne), new { id = credtedItem }, value);
            }
            catch (Exception e)
            {
                return errorHandler.CreateInternalServerErrorResponce(e.Message);
            }
        }

        [HttpPatch("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public override async Task<ActionResult<OrderItem>> Patch(int id, [FromBody] OrderItemUpdateDto value)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return UnprocessableEntity(ModelState);
                }

                var existingEntity = await (from p in this.dbSet
                                            where p.Id == id
                                            select p).FirstOrDefaultAsync();

                if (existingEntity == null)
                {
                    return errorHandler.CreateUnprocessableEntityResponce(CreateNotFoundMessage<OrderItem>(id));
                }

                var existFKItem = await (from fk in this.dbFKSet
                                            where fk.Id == existingEntity.OrderId
                                            select fk).FirstOrDefaultAsync();

                if (existFKItem == null)
                {
                    return errorHandler.CreateUnprocessableEntityResponce(CreateNotFoundMessage<Order>(existingEntity.OrderId));
                }

                mapper.Map(value, existingEntity);

                var updatedItem = await orderItemService.UpdateOrderItem(existFKItem, existingEntity);

                return updatedItem;
            }
            catch (Exception e)
            {
                return errorHandler.CreateInternalServerErrorResponce(e.Message);
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public override async Task<ActionResult> Delete(int id)
        {
            try
            {
                var existingEntity = await (from p in this.dbSet
                                            where p.Id == id
                                            select p).FirstOrDefaultAsync();

                if (existingEntity == null)
                {
                    return errorHandler.CreateUnprocessableEntityResponce(CreateNotFoundMessage<OrderItem>(id));
                }

                var existFKItem = await (from fk in this.dbFKSet
                                         where fk.Id == existingEntity.OrderId
                                         select fk).FirstOrDefaultAsync();

                if (existFKItem == null)
                {
                    return errorHandler.CreateUnprocessableEntityResponce(CreateNotFoundMessage<Order>(existingEntity.OrderId));
                }

                await orderItemService.DeleteOrderItem(existFKItem, existingEntity);

                return NoContent();
            }
            catch (Exception e)
            {
                return errorHandler.CreateInternalServerErrorResponce(e.Message);
            }
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


