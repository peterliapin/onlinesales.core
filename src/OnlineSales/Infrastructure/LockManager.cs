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
    private static LockManager? instance;

    private LockManager()
    {
    }

    public static LockManager? GetNoWaitLock(string lockKey)
    {
        if (instance == null)
        {
            if (CanGetLock(lockKey))
            {
                instance = new LockManager();
            }
            else
            {
                return null;
            }
        }

        return instance;
    }

    public static PostgresDistributedLockHandle? GetSecondaryNoWaitLock(string lockKey)
    {
        using (var dbContext = new ApiDbContext())
        {
            var secondaryLock = new PostgresDistributedLock(new PostgresAdvisoryLockKey(lockKey, true), dbContext.Database.GetConnectionString() !);

            // pg_try_advisory_lock - Get the lock or skip if not available.
            return secondaryLock.TryAcquire();
        }
    }

    public static PostgresDistributedLockHandle? GetWaitLock(string lockKey)
    {
        using (var dbContext = new ApiDbContext())
        {
            var secondaryLock = new PostgresDistributedLock(new PostgresAdvisoryLockKey(lockKey, true), dbContext.Database.GetConnectionString() !);

            // pg_advisory_lock - Get or Wait for lock.
            return secondaryLock.Acquire();
        }
    }

    private static bool CanGetLock(string lockKey)
    {
        using (var dbContext = new ApiDbContext())
        {
            try
            {
                var taskLock = new PostgresDistributedLock(new PostgresAdvisoryLockKey(lockKey, true), dbContext.Database.GetConnectionString() !);

                // pg_try_advisory_lock - Get the lock or skip if not available.
                var handle = taskLock.TryAcquire();

                if (handle is null)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error when acquiring lock.");
                return false;
            }
        }
    }
}
