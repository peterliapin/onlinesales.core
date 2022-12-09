// <copyright file="OrderItemService.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Transactions;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OnlineSales.Data;
using OnlineSales.DTOs;
using OnlineSales.Entities;
using OnlineSales.Interfaces;

namespace OnlineSales.Services
{
    public class OrderItemService : IOrderItemService
    {
        private readonly ApiDbContext apiDbContext;
        private readonly IMapper mapper;

        public OrderItemService(ApiDbContext apiDbContext, IMapper mapper)
        {
            this.apiDbContext = apiDbContext;
            this.mapper = mapper;
        }

        public async Task<int> AddOrderItem(OrderItemCreateDto orderItemCreateDto)
        {
            var orderItem = mapper.Map<OrderItem>(orderItemCreateDto);

            var order = await (from ord in apiDbContext.Orders! where ord.Id == orderItem.OrderId select ord).FirstOrDefaultAsync();

            if (order == null)
            {
                throw new KeyNotFoundException("Expected order  not found");
            }

            if (order!.ExchangeRateToPayOutCurrency == 0)
            {
                order.ExchangeRateToPayOutCurrency = orderItemCreateDto.ExchangeRateToPayOutCurrency;
            }

            orderItem.CurrencyTotal = CalculateOrderItemCurrencyTotal(orderItem);
            orderItem.Total = CalculateOrderItemTotal(orderItem, order!.ExchangeRateToPayOutCurrency);

            using (var transaction = await apiDbContext!.Database.BeginTransactionAsync())
            {
                await apiDbContext.AddAsync(orderItem);

                var totals = CalculateTotalsForOrder(orderItem);

                order.CurrencyTotal = totals.currencyTotal;
                order.Total = totals.total;

                if (!string.IsNullOrEmpty(orderItemCreateDto.Data))
                {
                    order.Data = orderItemCreateDto.Data; 
                }

                apiDbContext.Update(order);
                await apiDbContext.SaveChangesAsync();

                await transaction.CommitAsync();
            }

            return orderItem.Id;
        }

        public async Task<OrderItem> UpdateOrderItem(int orderItemId, OrderItemUpdateDto orderItemUpdateDto)
        {
            var orderItemExist = await (from ordItem in apiDbContext.OrderItems where ordItem.Id == orderItemId select ordItem).FirstOrDefaultAsync();

            if (orderItemExist == null)
            {
                throw new KeyNotFoundException("Expected order item id not found");
            }

            var order = await (from ord in apiDbContext.Orders! where ord.Id == orderItemExist!.OrderId select ord).FirstOrDefaultAsync();

            if (order == null)
            {
                throw new KeyNotFoundException("Requested order is not found");
            }

            var mergedItem = mapper.Map(orderItemUpdateDto, orderItemExist);

            mergedItem!.CurrencyTotal = CalculateOrderItemCurrencyTotal(mergedItem);
            mergedItem!.Total = CalculateOrderItemTotal(mergedItem, order!.ExchangeRateToPayOutCurrency);

            using (var transaction = await apiDbContext!.Database.BeginTransactionAsync())
            {
                apiDbContext.Update(mergedItem);

                var totals = CalculateTotalsForOrder(mergedItem!, orderItemId);

                order.CurrencyTotal = totals.currencyTotal;
                order.Total = totals.total;

                if (!string.IsNullOrEmpty(orderItemUpdateDto.Data))
                {
                    order.Data = orderItemUpdateDto.Data;
                }

                apiDbContext.Update(order);
                await apiDbContext.SaveChangesAsync();

                await transaction.CommitAsync();
            }

            return mergedItem;
        }

        private decimal CalculateOrderItemCurrencyTotal(OrderItem orderItem)
        {
            return orderItem.UnitPrice * orderItem.Quantity;
        }

        private decimal CalculateOrderItemTotal(OrderItem orderItem, decimal exchangeRate)
        {
            return orderItem.CurrencyTotal * exchangeRate;
        }

        private (decimal currencyTotal, decimal total) CalculateTotalsForOrder(OrderItem orderItem, int patchId = 0)
        {
            decimal currencyTotal = 0;
            decimal total = 0;
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

            currencyTotal += orderItem.CurrencyTotal;
            total += orderItem.Total;

            return (currencyTotal, total);
        }
    }
}
