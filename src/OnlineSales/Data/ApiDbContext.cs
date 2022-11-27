// <copyright file="ApiDbContext.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using Microsoft.EntityFrameworkCore;
using OnlineSales.Configuration;
using OnlineSales.Entities;

namespace OnlineSales.Data;

public class ApiDbContext : DbContext
{
    protected readonly IConfiguration configuration;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiDbContext"/> class.
    /// Constructor with no parameters and manual configuration building is required for the case when you would like to use ApiDbContext as a base class for a new context (let's say in a plugin).
    /// </summary>
    public ApiDbContext()
    {
        this.configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", false)
            .AddEnvironmentVariables()
            .AddUserSecrets(typeof(Program).Assembly)
            .Build();
    }

    public ApiDbContext(DbContextOptions<ApiDbContext> options, IConfiguration configuration)
        : base(options)
    {
        this.configuration = configuration;
    }

    public virtual DbSet<Post>? Posts { get; set; }

    public virtual DbSet<Comment>? Comments { get; set; }

    public virtual DbSet<Customer>? Customers { get; set; }

    public virtual DbSet<Order>? Orders { get; set; }

    public virtual DbSet<OrderItem>? OrderItems { get; set; }

    public virtual DbSet<TaskExecutionLog>? TaskExecutionLogs { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var postgresConfig = configuration.GetSection("Postgres").Get<PostgresConfig>();

        if (postgresConfig == null)
        {
            throw new MissingConfigurationException("Postgres configuraiton is mandatory.");
        }

        optionsBuilder.UseNpgsql(
            postgresConfig.ConnectionString,
            b => b.MigrationsHistoryTable("_migrations"))
        .UseSnakeCaseNamingConvention();
    }
}