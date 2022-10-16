// <copyright file="IBaseRepository.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>
using Nest;
using OnlineSales.Models;

namespace OnlineSales.Repositories
{
    public interface IBaseRepository<T>
        where T : BaseEntity
    {
        Task<IEnumerable<T>> GetAll();

        Task<T?> GetById(int id);

        Task<T> Add(T entity);

        Task<T> Update(T entity);

        Task<bool> Delete(int id);
    }
}

