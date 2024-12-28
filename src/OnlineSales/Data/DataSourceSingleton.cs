// <copyright file="DataSourceSingleton.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using Npgsql;
using OnlineSales.Configuration;

namespace OnlineSales.Data;

public static class DataSourceSingleton
{
    private static NpgsqlDataSource? instance = null;

    public static NpgsqlDataSource GetInstance(IConfiguration configuration)
    {
        instance ??= BuildDataSource(configuration);
        return instance;
    }

    private static NpgsqlDataSource BuildDataSource(IConfiguration configuration)
    {
        var postgresConfig = configuration.GetSection("Postgres").Get<PostgresConfig>();

        if (postgresConfig == null)
        {
            throw new MissingConfigurationException("Postgres configuration is mandatory.");
        }

        var dataSourceBuilder = new NpgsqlDataSourceBuilder(postgresConfig.ConnectionString);
        dataSourceBuilder.EnableDynamicJson();
        return dataSourceBuilder.Build();
    }
}
