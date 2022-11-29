// <copyright file="ApiDbContext.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using Microsoft.EntityFrameworkCore;
using OnlineSales.Configuration;
using OnlineSales.Entities;
using OnlineSales.Interfaces;

namespace OnlineSales.Data;

public class ApiDbContext : DbContext
{
    protected readonly IConfiguration configuration;

    private readonly IHttpContextHelper? httpContextHelper;

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

    public ApiDbContext(DbContextOptions<ApiDbContext> options, IConfiguration configuration, IHttpContextHelper httpContextHelper)
        : base(options)
    {
        this.configuration = configuration;
        this.httpContextHelper = httpContextHelper;
    }

    public virtual DbSet<Post>? Posts { get; set; }

    public virtual DbSet<Comment>? Comments { get; set; }

    public virtual DbSet<Customer>? Customers { get; set; }

    public virtual DbSet<Order>? Orders { get; set; }

    public virtual DbSet<OrderItem>? OrderItems { get; set; }

    public virtual DbSet<TaskExecutionLog>? TaskExecutionLogs { get; set; }

    public virtual DbSet<Image>? Images { get; set; }

    public virtual DbSet<EmailGroup>? EmailGroups { get; set; }

    public virtual DbSet<EmailSchedule>? EmailSchedules { get; set; }

    public virtual DbSet<EmailTemplate>? EmailTemplates { get; set; }

    public virtual DbSet<CustomerEmailSchedule>? CustomerEmailSchedules { get; set; }

    public virtual DbSet<CustomerEmailLog>? CustomerEmailLogs { get; set; }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker
       .Entries()
       .Where(e => e.Entity is BaseEntity && (
               e.State == EntityState.Added
               || e.State == EntityState.Modified));

        foreach (var entityEntry in entries)
        {
            if (entityEntry.State == EntityState.Modified)
            {
                ((BaseEntity)entityEntry.Entity).UpdatedAt = DateTime.UtcNow;
                ((BaseEntity)entityEntry.Entity).UpdatedByIP = httpContextHelper!.IpAddress;
                ((BaseEntity)entityEntry.Entity).UpdatedByUserAgent = httpContextHelper!.UserAgent;
            }

            if (entityEntry.State == EntityState.Added)
            {
                ((BaseEntity)entityEntry.Entity).CreatedAt = DateTime.UtcNow;
                ((BaseEntity)entityEntry.Entity).CreatedByIP = httpContextHelper!.IpAddress;
                ((BaseEntity)entityEntry.Entity).CreatedByUserAgent = httpContextHelper!.UserAgent;
            }
        }

        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

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