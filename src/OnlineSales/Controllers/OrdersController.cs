// <copyright file="OrdersController.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSales.Data;
using OnlineSales.DTOs;
using OnlineSales.Entities;

namespace OnlineSales.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : BaseController<Order, OrderCreateDto, OrderUpdateDto>
    {
        private new readonly ApiDbContext dbContext;

        public OrdersController(ApiDbContext dbContext, IMapper mapper)
            : base(dbContext, mapper)
        {
            this.dbContext = dbContext;
        }

        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public override async Task<ActionResult> Delete(int id)
        {
            try
            {
                var order = await (from ord in dbContext.Orders! where ord.Id == id select ord).FirstOrDefaultAsync();

                if (order == null)
                {
                    return NotFound();
                }

                using (var txnScope = await dbContext.Database.BeginTransactionAsync())
                {
                    var orderItems = (from ordItem in dbContext.OrderItems where ordItem.OrderId == id select ordItem).ToList();

                    if (orderItems.Count > 0)
                    {
                        dbContext.RemoveRange(orderItems!);
                    }

                    dbContext.Remove(order);
                    await dbContext.SaveChangesAsync();

                    await txnScope.CommitAsync();
                }

                return Ok();
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
