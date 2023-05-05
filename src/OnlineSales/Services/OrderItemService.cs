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

        public async Task AddAsync(Order order, OrderItem orderItem)
        {
            orderItem.CurrencyTotal = this.CalculateOrderItemCurrencyTotal(orderItem);
            orderItem.Total = this.CalculateOrderItemTotal(orderItem, order.ExchangeRate);

            await this.pgDbContext.AddAsync(orderItem);

            var totals = this.CalculateTotalsForOrder(orderItem);

            order.CurrencyTotal = totals.currencyTotal;
            order.Total = totals.total;
            order.Quantity = totals.quantity;
        }

        public async Task DeleteAsync(Order order, OrderItem orderItem)
        {
            using (var transaction = await this.pgDbContext!.Database.BeginTransactionAsync())
            {
                this.pgDbContext.Remove(orderItem);

                orderItem.CurrencyTotal = 0;
                orderItem.Total = 0;
                orderItem.Quantity = 0;

                var totals = this.CalculateTotalsForOrder(orderItem);

                order.CurrencyTotal = totals.currencyTotal;
                order.Total = totals.total;

                this.pgDbContext.Update(order);
                await this.pgDbContext.SaveChangesAsync();

                await transaction.CommitAsync();
            }
        }

        public void Update(Order order, OrderItem orderItem)
        {
            orderItem.CurrencyTotal = this.CalculateOrderItemCurrencyTotal(orderItem);
            orderItem.Total = this.CalculateOrderItemTotal(orderItem, order!.ExchangeRate);

            var totals = this.CalculateTotalsForOrder(orderItem, orderItem.Id);

            order.CurrencyTotal = totals.currencyTotal;
            order.Total = totals.total;
            order.Quantity = totals.quantity;
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
            var quantity = 0;

            var orderItems = patchId == 0
                ? (from ordItem in this.pgDbContext.OrderItems where ordItem.OrderId == orderItem.OrderId select ordItem).ToList()
                : (from ordItem in this.pgDbContext.OrderItems where ordItem.OrderId == orderItem.OrderId && ordItem.Id != patchId select ordItem).ToList();

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