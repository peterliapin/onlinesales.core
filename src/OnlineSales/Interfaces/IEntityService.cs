// <copyright file="IEntityService.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using OnlineSales.Data;
using OnlineSales.Entities;

namespace OnlineSales.Interfaces
{
    public interface IEntityService<T>
        where T : BaseEntityWithId
    {
        Task SaveAsync(T item);

        Task SaveRangeAsync(List<T> items);

        void SetDBContext(PgDbContext pgDbContext);
    }
}