// <copyright file="CommentsRepository.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>
using OnlineSales.Data;
using OnlineSales.Models;

namespace OnlineSales.Repositories;

public class CommentsRepository : BaseRepository<Comment>
{
    public CommentsRepository(ApiDbContext dbContext, ILogger logger)
        : base(dbContext, logger)
    {
    }
}

