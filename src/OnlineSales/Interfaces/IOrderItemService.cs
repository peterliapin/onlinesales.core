// <copyright file="IOrderItemService.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using OnlineSales.DTOs;
using OnlineSales.Entities;

namespace OnlineSales.Interfaces
{
    public interface IOrderItemService
    {
        Task<int> AddOrderItem(OrderItemCreateDto orderItemCreateDto);

        Task<OrderItem> UpdateOrderItem(int orderItemId, OrderItemUpdateDto orderItemUpdateDto);

        Task DeleteOrderItem(int orderItemId);
    }
}
