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
    [ApiController]
    public class OrderItemsController : BaseController<OrderItem, OrderItemCreateDto, OrderItemUpdateDto>
    {
        private readonly IOrderItemService orderItemService;

        public OrderItemsController(ApiDbContext dbContext, IMapper mapper, IOrderItemService orderItemService)
            : base(dbContext, mapper)
        {
            this.orderItemService = orderItemService;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public override async Task<ActionResult<OrderItem>> Post([FromBody] OrderItemCreateDto value)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return UnprocessableEntity(ModelState);
                }

                var orderItem = mapper.Map<OrderItem>(value);

                var credtedItem = await orderItemService.AddOrderItem(orderItem);
                return CreatedAtAction(nameof(GetOne), new { id = credtedItem }, value);
            }
            catch (KeyNotFoundException knfe)
            {
                return NotFound(knfe.Message);
            }
            catch (Exception ex)
            {
                return Problem(
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: ex.Message);
            }
        }

        [HttpPatch("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public override async Task<ActionResult<OrderItem>> Patch(int id, [FromBody] OrderItemUpdateDto value)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return UnprocessableEntity(ModelState);
                }

                var existingOrderItem = await (from p in this.dbSet
                                            where p.Id == id
                                            select p).FirstOrDefaultAsync();

                if (existingOrderItem == null)
                {
                    return NotFound();
                }

                mapper.Map(value, existingOrderItem);

                var updatedItem = await orderItemService.UpdateOrderItem(existingOrderItem);

                return updatedItem;
            }
            catch (KeyNotFoundException knfe)
            {
                return NotFound(knfe.Message);
            }
            catch (Exception ex)
            {
                return Problem(
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: ex.Message);
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
                await orderItemService.DeleteOrderItem(id);

                return NoContent();
            }
            catch (KeyNotFoundException knfe)
            {
                return NotFound(knfe.Message);
            }
            catch (Exception ex)
            {
                return Problem(
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: ex.Message);
            }
        }
    }
}


