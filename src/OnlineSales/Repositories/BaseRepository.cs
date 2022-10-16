// <copyright file="BaseRepository.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>
using Microsoft.EntityFrameworkCore;
using OnlineSales.Data;
using OnlineSales.Models;

namespace OnlineSales.Repositories;

public abstract class BaseRepository<T> : IBaseRepository<T>
    where T : BaseEntity
{
    protected readonly ApiDbContext dbContext;
    protected readonly ILogger logger;
    protected readonly DbSet<T> dbSet;

    protected BaseRepository(ApiDbContext dbContext, ILogger logger)
    {
        this.dbContext = dbContext;
        this.logger = logger;
        this.dbSet = dbContext.Set<T>();
    }

    public async Task<T> Add(T entity)
    {
        var result = await dbSet.AddAsync(entity);

        return result.Entity;
    }

    public Task<T> Update(T entity)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<T>> GetAll()
    {
        return await dbSet.ToListAsync();
    }

    public async Task<T?> GetById(int id)
    {
        return await dbSet.FindAsync(id);
    }

    public async Task<bool> Delete(int id)
    {
        var entity = await GetById(id);

        if (entity != null)
        {
            dbSet.Remove(entity);
            return true;
        }
        else
        {
            return false;
        }
    }
}

