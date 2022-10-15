// <copyright file="IRepository.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>
using OnlineSales.Models;

namespace OnlineSales.Repositories
{
    public interface IRepository<T>
        where T : BaseEntity
    {
        IEnumerable<T> GetAll();

        T GetById(string id);

        void Add(T instance);

        void Update(T instance);

        void Delete(string id);

        void Save();
    }
}

