// <copyright file="PostsRepository.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using OnlineSales.Data;
using OnlineSales.Entities;

namespace OnlineSales.Repositories;

public class PostsRepository : BaseRepository<Post>
{
    public PostsRepository(ApiDbContext dbContext, ILogger logger)
        : base(dbContext, logger)
    {
    }
}

