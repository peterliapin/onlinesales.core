// <copyright file="LockManager.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using Medallion.Threading.Postgres;
using Microsoft.EntityFrameworkCore;
using Nest;
using OnlineSales.Data;

namespace OnlineSales.Infrastructure;

public class LockManager
{
    private readonly ApiDbContext dbContext;

    public LockManager(ApiDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public PostgresDistributedLockHandle? GetNoWaitLock(string lockKey)
    {
        try
        {
            var secondaryLock = new PostgresDistributedLock(new PostgresAdvisoryLockKey(lockKey, true), dbContext.Database.GetConnectionString() !);

            // pg_try_advisory_lock - Get the lock or skip if not available.
            return secondaryLock.TryAcquire();
        }
        catch (Exception ex)
        {
            LogError(ex, lockKey);
            return null;
        }
    }

    public PostgresDistributedLockHandle? GetWaitLock(string lockKey)
    {
        try
        {
            var secondaryLock = new PostgresDistributedLock(new PostgresAdvisoryLockKey(lockKey, true), dbContext.Database.GetConnectionString() !);

            // pg_advisory_lock - Get or Wait for lock.
            return secondaryLock.Acquire();
        }
        catch (Exception ex)
        {
            LogError(ex, lockKey);
            return null;
        }
    }

    private static void LogError(Exception ex, string lockKey)
    {
        Log.Error(ex, "Error when acquiring lock:" + lockKey);
    }
}
