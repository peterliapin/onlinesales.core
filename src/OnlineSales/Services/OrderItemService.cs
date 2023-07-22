// <copyright file="OrderItemService.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using OnlineSales.Data;
using OnlineSales.Entities;
using OnlineSales.Interfaces;

namespace OnlineSales.Services
{
    public class OrderItemService : IOrderItemService
    {        
        private readonly IOrderService orderService;
        private PgDbContext pgDbContext;

        public OrderItemService(PgDbContext pgDbContext, IOrderService orderService)
        {
            this.pgDbContext = pgDbContext;
            this.orderService = orderService;
        }

        public void Delete(OrderItem orderItem)
        {
            pgDbContext.Remove(orderItem);
            orderService.RecalculateOrder(orderItem.Order!);
        }

        public async Task SaveAsync(OrderItem orderItem)
        {
            orderItem.CurrencyTotal = CalculateOrderItemCurrencyTotal(orderItem);
            orderItem.Total = CalculateOrderItemTotal(orderItem, orderItem.Order!.ExchangeRate);

            if (orderItem.Id > 0)
            {
                pgDbContext.OrderItems!.Update(orderItem);
            }
            else
            {
                await pgDbContext.OrderItems!.AddAsync(orderItem);
            }

            orderService.RecalculateOrder(orderItem.Order!);
        }

        public Task SaveRangeAsync(List<OrderItem> items)
        {
            throw new NotImplementedException();
        }

        public void SetDBContext(PgDbContext pgDbContext)
        {
            this.pgDbContext = pgDbContext;
        }

        private decimal CalculateOrderItemCurrencyTotal(OrderItem orderItem)
        {
            return orderItem.UnitPrice * orderItem.Quantity;
        }

        private decimal CalculateOrderItemTotal(OrderItem orderItem, decimal exchangeRate)
        {
            return orderItem.CurrencyTotal * exchangeRate;
        }
    }
}