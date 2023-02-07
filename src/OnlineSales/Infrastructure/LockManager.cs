// <copyright file="LockManager.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using Medallion.Threading.Postgres;
using Microsoft.EntityFrameworkCore;
using OnlineSales.Data;

namespace OnlineSales.Infrastructure;

public class LockManager
{
    private readonly PgDbContext dbContext;

    public LockManager(PgDbContext dbContext)
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
            Log.Error(ex, "Error when acquiring lock.");
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
            Log.Error(ex, "Error when acquiring lock.");
            return null;
        }
    }
}