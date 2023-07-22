// <copyright file="OrderService.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using OnlineSales.Data;
using OnlineSales.Entities;

namespace OnlineSales.Services
{
    public class OrderService 
    {
        private readonly PgDbContext pgDbContext;

        public OrderService(PgDbContext pgDbContext)
        {
            this.pgDbContext = pgDbContext;
        }

        public void RecalculateOrder(Order order)
        {
            if (order.OrderItems == null)
            {
                pgDbContext.Entry(order).Collection(o => o.OrderItems!).Load();
            }

            if (order.Discounts == null)
            {
                pgDbContext.Entry(order).Collection(o => o.Discounts!).Load();
            }

            var itemsCurrencyTotalSum = order.OrderItems!.Sum(oi => oi.CurrencyTotal);
            order.CurrencyTotal = itemsCurrencyTotalSum - order.Discounts!.Sum(d => d.Value) - order.Refund;

            order.Total = order.CurrencyTotal * order.ExchangeRate;
            order.Quantity = order.OrderItems!.Sum(oi => oi.Quantity);
        }
    }
}