// <copyright file="IQueryProvider.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using OnlineSales.Entities;

namespace OnlineSales.Infrastructure
{
    public interface IQueryProvider<T>
        where T : BaseEntityWithId
    {
        public Task<QueryResult<T>> GetResult();
    }

    public class QueryResult<T>
        where T : BaseEntityWithId
    {
        public QueryResult(IList<T>? records, long totalCount)
        {
            Records = records;
            TotalCount = totalCount;
        }

        public IList<T>? Records { get; init; }

        public long TotalCount { get; init; }
    }
}
