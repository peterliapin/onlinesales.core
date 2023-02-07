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
        private readonly PgDbContext pgDbContext;

        public OrderItemService(PgDbContext pgDbContext)
        {
            this.pgDbContext = pgDbContext;
        }

        public async Task<int> AddOrderItem(Order order, OrderItem orderItem)
        {
            orderItem.CurrencyTotal = CalculateOrderItemCurrencyTotal(orderItem);
            orderItem.Total = CalculateOrderItemTotal(orderItem, order.ExchangeRate);

            using (var transaction = await pgDbContext!.Database.BeginTransactionAsync())
            {
                await pgDbContext.AddAsync(orderItem);

                var totals = CalculateTotalsForOrder(orderItem);

                order.CurrencyTotal = totals.currencyTotal;
                order.Total = totals.total;
                order.Quantity = totals.quantity;

                pgDbContext.Update(order);
                await pgDbContext.SaveChangesAsync();

                await transaction.CommitAsync();
            }

            return orderItem.Id;
        }

        public async Task DeleteOrderItem(Order order, OrderItem orderItem)
        {
            using (var transaction = await pgDbContext!.Database.BeginTransactionAsync())
            {
                pgDbContext.Remove(orderItem);

                orderItem.CurrencyTotal = 0;
                orderItem.Total = 0;
                orderItem.Quantity = 0;

                var totals = CalculateTotalsForOrder(orderItem);

                order.CurrencyTotal = totals.currencyTotal;
                order.Total = totals.total;

                pgDbContext.Update(order);
                await pgDbContext.SaveChangesAsync();

                await transaction.CommitAsync();
            }
        }

        public async Task<OrderItem> UpdateOrderItem(Order order, OrderItem orderItem)
        {
            orderItem.CurrencyTotal = CalculateOrderItemCurrencyTotal(orderItem);
            orderItem.Total = CalculateOrderItemTotal(orderItem, order!.ExchangeRate);

            using (var transaction = await pgDbContext!.Database.BeginTransactionAsync())
            {
                pgDbContext.Update(orderItem);

                var totals = CalculateTotalsForOrder(orderItem, orderItem.Id);

                order.CurrencyTotal = totals.currencyTotal;
                order.Total = totals.total;
                order.Quantity = totals.quantity;

                pgDbContext.Update(order);
                await pgDbContext.SaveChangesAsync();

                await transaction.CommitAsync();
            }

            return orderItem;
        }

        private decimal CalculateOrderItemCurrencyTotal(OrderItem orderItem)
        {
            return orderItem.UnitPrice * orderItem.Quantity;
        }

        private decimal CalculateOrderItemTotal(OrderItem orderItem, decimal exchangeRate)
        {
            return orderItem.CurrencyTotal * exchangeRate;
        }

        private (decimal currencyTotal, decimal total, int quantity) CalculateTotalsForOrder(OrderItem orderItem, int patchId = 0)
        {
            decimal currencyTotal = 0;
            decimal total = 0;
            int quantity = 0;
            
            var orderItems = patchId == 0
                ? (from ordItem in pgDbContext.OrderItems where ordItem.OrderId == orderItem.OrderId select ordItem).ToList()
                : (from ordItem in pgDbContext.OrderItems where ordItem.OrderId == orderItem.OrderId && ordItem.Id != patchId select ordItem).ToList();

            currencyTotal = orderItems.Sum(t => t.CurrencyTotal);
            total = orderItems.Sum(t => t.Total);
            quantity = orderItems.Sum(t => t.Quantity);

            currencyTotal += orderItem.CurrencyTotal;
            total += orderItem.Total;
            quantity += orderItem.Quantity;

            return (currencyTotal, total, quantity);
        }
    }
}