// <copyright file="IOrderItemService.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using OnlineSales.Entities;

namespace OnlineSales.Interfaces
{
    public interface IOrderItemService
    {
        Task<int> AddOrderItem(Order order, OrderItem orderItem);

        Task<OrderItem> UpdateOrderItem(Order order, OrderItem orderItem);

        Task DeleteOrderItem(Order order, OrderItem orderItem);
    }
}