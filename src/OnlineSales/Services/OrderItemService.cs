// <copyright file="OrderItemService.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using Microsoft.EntityFrameworkCore;
using OnlineSales.Data;
using OnlineSales.Entities;
using OnlineSales.Interfaces;

namespace OnlineSales.Services
{
    public class OrderItemService : IOrderItemService
    {
        private readonly ApiDbContext apiDbContext;

        public OrderItemService(ApiDbContext apiDbContext)
        {
            this.apiDbContext = apiDbContext;
        }

        public async Task<int> AddOrderItem(OrderItem orderItem)
        {
            var order = await (from ord in apiDbContext.Orders! where ord.Id == orderItem.OrderId select ord).FirstOrDefaultAsync();

            if (order == null)
            {
                throw new KeyNotFoundException($"Order with Id = {orderItem.OrderId} not found");
            }

            orderItem.CurrencyTotal = CalculateOrderItemCurrencyTotal(orderItem);
            orderItem.Total = CalculateOrderItemTotal(orderItem, order.ExchangeRate);

            using (var transaction = await apiDbContext!.Database.BeginTransactionAsync())
            {
                await apiDbContext.AddAsync(orderItem);

                var totals = CalculateTotalsForOrder(orderItem);

                order.CurrencyTotal = totals.currencyTotal;
                order.Total = totals.total;
                order.Quantity = totals.quantity;

                apiDbContext.Update(order);
                await apiDbContext.SaveChangesAsync();

                await transaction.CommitAsync();
            }

            return orderItem.Id;
        }

        public async Task DeleteOrderItem(int orderItemId)
        {
            var orderItemExist = await (from ordItem in apiDbContext.OrderItems where ordItem.Id == orderItemId select ordItem).FirstOrDefaultAsync();

            if (orderItemExist == null)
            {
                throw new KeyNotFoundException($"Order item with Id = {orderItemId} not found");
            }

            var order = await (from ord in apiDbContext.Orders! where ord.Id == orderItemExist.OrderId select ord).FirstOrDefaultAsync();

            if (order == null)
            {
                throw new KeyNotFoundException($"Order with Id = {orderItemExist.OrderId} not found");
            }

            using (var transaction = await apiDbContext!.Database.BeginTransactionAsync())
            {
                apiDbContext.Remove(orderItemExist);

                orderItemExist.CurrencyTotal = 0;
                orderItemExist.Total = 0;
                orderItemExist.Quantity = 0;

                var totals = CalculateTotalsForOrder(orderItemExist!);

                order.CurrencyTotal = totals.currencyTotal;
                order.Total = totals.total;

                apiDbContext.Update(order);
                await apiDbContext.SaveChangesAsync();

                await transaction.CommitAsync();
            }
        }

        public async Task<OrderItem> UpdateOrderItem(OrderItem orderItem)
        {
            var order = await (from ord in apiDbContext.Orders! where ord.Id == orderItem.OrderId select ord).FirstOrDefaultAsync();

            if (order == null)
            {
                throw new KeyNotFoundException($"Order with Id = {orderItem.OrderId} not found");
            }

            orderItem.CurrencyTotal = CalculateOrderItemCurrencyTotal(orderItem);
            orderItem.Total = CalculateOrderItemTotal(orderItem, order!.ExchangeRate);

            using (var transaction = await apiDbContext!.Database.BeginTransactionAsync())
            {
                apiDbContext.Update(orderItem);

                var totals = CalculateTotalsForOrder(orderItem, orderItem.Id);

                order.CurrencyTotal = totals.currencyTotal;
                order.Total = totals.total;
                order.Quantity = totals.quantity;

                apiDbContext.Update(order);
                await apiDbContext.SaveChangesAsync();

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

            List<OrderItem> orderItems;

            if (patchId == 0)
            {
                orderItems = (from ordItem in apiDbContext.OrderItems where ordItem.OrderId == orderItem.OrderId select ordItem).ToList();
            }
            else
            {
                orderItems = (from ordItem in apiDbContext.OrderItems where ordItem.OrderId == orderItem.OrderId && ordItem.Id != patchId select ordItem).ToList();
            }

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
