// <copyright file="ISaveService.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using OnlineSales.Entities;

namespace OnlineSales.Interfaces
{
    public interface ISaveService<T>
        where T : BaseEntityWithId
    {
        Task SaveAsync(T item);

        Task SaveRangeAsync(List<T> items);
    }
}