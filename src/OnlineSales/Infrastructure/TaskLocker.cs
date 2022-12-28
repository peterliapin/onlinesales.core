// <copyright file="TaskLocker.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using Medallion.Threading.Postgres;
using Microsoft.EntityFrameworkCore;
using OnlineSales.Data;

namespace OnlineSales.Infrastructure;

public class TaskLocker
{
    protected static readonly object LockObj = new object();

    private static TaskLocker? instance;

    private TaskLocker()
    {
    }

    public static TaskLocker? GetInstance(string lockKey)
    {
        if (instance == null)
        {
            lock (LockObj)
            {
                if (CanGetLock(lockKey))
                {
                    if (instance == null)
                    {
                        instance = new TaskLocker();
                    }
                }
                else
                {
                    return null;
                }
            }
        }

        return instance;
    }

    private static bool CanGetLock(string lockKey)
    {
        using (var dbContext = new ApiDbContext())
        {
            try
            {
                var taskLock = new PostgresDistributedLock(new PostgresAdvisoryLockKey(lockKey, true), dbContext.Database.GetConnectionString() !);

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
