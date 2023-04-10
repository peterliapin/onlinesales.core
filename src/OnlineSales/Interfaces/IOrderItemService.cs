// <copyright file="IOrderItemService.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using OnlineSales.Entities;

namespace OnlineSales.Interfaces
{
    public interface IOrderItemService
    {
        Task AddAsync(Order order, OrderItem orderItem);

        void Update(Order order, OrderItem orderItem);

        Task DeleteAsync(Order order, OrderItem orderItem);
    }
}