// <copyright file="LockService.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using Medallion.Threading.Postgres;
using OnlineSales.Configuration;
using OnlineSales.Interfaces;

namespace OnlineSales.Services;

public class LockService : ILockService
{
    private readonly string connectionSting;

    public LockService(IConfiguration configuration)
    {
        var postgresConfig = configuration.GetSection("Postgres").Get<PostgresConfig>();

        if (postgresConfig == null)
        {
            throw new MissingConfigurationException("Postgres configuration is mandatory.");
        }

        this.connectionSting = postgresConfig.ConnectionString;
    }

    public ILockHolder Lock(string key)
    {
        throw new NotImplementedException();
    }

    public ILockHolder? TryLock(string key)
    {
        var secondaryLock = new PostgresDistributedLock(new PostgresAdvisoryLockKey(key, true), this.connectionSting);

        var postgresDistributedLock = secondaryLock.TryAcquire();

        if (postgresDistributedLock is null)
        {
            return null;
        }
        else
        {
            return new PostgresLockHolder();
        }
    }
}

public class PostgresLockHolder : ILockHolder
{
    public PostgresLockHolder()
    {
    }
}