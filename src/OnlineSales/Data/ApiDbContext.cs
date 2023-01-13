// <copyright file="ApiDbContext.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
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
        try
        {
            Console.WriteLine("Initializing ApiDbContext...");

            this.configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", false)
                .AddEnvironmentVariables()
                .AddUserSecrets(typeof(Program).Assembly)
                .Build();

            Console.WriteLine("ApiDbContext initialized");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Failed to create ApiDbContext. Error: {0}, Stack Trace: {1}", ex.Message, ex.StackTrace);
            throw;
        }
    }

    public ApiDbContext(DbContextOptions<ApiDbContext> options, IConfiguration configuration, IHttpContextHelper httpContextHelper)
        : base(options)
    {
        this.configuration = configuration;
        this.httpContextHelper = httpContextHelper;
    }

    public bool IsImportRequest { get; set; }

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

    public virtual DbSet<EmailLog>? EmailLogs { get; set; }

    public virtual DbSet<IpDetails>? IpDetails { get; set; }

    public virtual DbSet<ChangeLog>? ChangeLogs { get; set; }

    public virtual DbSet<ChangeLogTaskLog>? ChangeLogTaskLogs { get; set; }

    public virtual DbSet<Link>? Links { get; set; }

    public virtual DbSet<LinkLog>? LinkLogs { get; set; }

    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        Dictionary<EntityEntry, ChangeLog> changes = new ();

        var entries = ChangeTracker
       .Entries()
       .Where(e => (e.Entity is BaseCreateByEntity) && (
               e.State == EntityState.Added
               || e.State == EntityState.Modified
               || e.State == EntityState.Deleted));

        if (entries.Any())
        {
            foreach (var entityEntry in entries)
            {
                if (entityEntry.State == EntityState.Modified && entityEntry.Entity is BaseEntity)
                {
                    var entity = (BaseEntity)entityEntry.Entity;
                    entity.UpdatedAt = IsImportRequest && entity.UpdatedAt is not null ? GetDateWithKind(entity.UpdatedAt.Value) : DateTime.UtcNow;
                    entity.CreatedAt = IsImportRequest ? GetDateWithKind(entity.CreatedAt) : entity.CreatedAt;
                    entity.UpdatedByIp = IsImportRequest && !string.IsNullOrEmpty(entity.UpdatedByIp) ? entity.UpdatedByIp : httpContextHelper!.IpAddress;
                    entity.UpdatedByUserAgent = IsImportRequest && !string.IsNullOrEmpty(entity.UpdatedByUserAgent) ? entity.UpdatedByUserAgent : httpContextHelper!.UserAgent;
                }

                if (entityEntry.State == EntityState.Added)
                {
                    var entity = (BaseCreateByEntity)entityEntry.Entity;
                    entity.CreatedAt = entity.CreatedAt == DateTime.MinValue ? DateTime.UtcNow : GetDateWithKind(entity.CreatedAt);
                    entity.CreatedByIp = string.IsNullOrEmpty(entity.CreatedByIp) ? httpContextHelper!.IpAddress : entity.CreatedByIp;
                    entity.CreatedByUserAgent = string.IsNullOrEmpty(entity.CreatedByUserAgent) ? httpContextHelper!.UserAgent : entity.CreatedByUserAgent;
                }

                // save entity state as it is before SaveChanges call
                changes[entityEntry] = new ChangeLog
                {
                    ObjectType = entityEntry.Entity.GetType().Name,
                    EntityState = entityEntry.State,
                };
            }

            await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);

            foreach (var change in changes)
            {
                // save object id which we only recieve after SaveChanges (for new records)
                change.Value.ObjectId = ((BaseEntityWithId)change.Key.Entity).Id;
                change.Value.Data = JsonSerializer.Serialize(change.Key.Entity);
            }

            ChangeLogs!.AddRange(changes.Values);
        } 

        return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        try
        {
            Console.WriteLine("Configuring ApiDbContext...");

            var postgresConfig = configuration.GetSection("Postgres").Get<PostgresConfig>();

            if (postgresConfig == null)
            {
                throw new MissingConfigurationException("Postgres configuration is mandatory.");
            }

            optionsBuilder.UseNpgsql(
                postgresConfig.ConnectionString,
                b => b.MigrationsHistoryTable("_migrations"))
            .UseSnakeCaseNamingConvention();

            Console.WriteLine("ApiDbContext successfully configured");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Failed to configure ApiDbContext. Error: {0}, Stack Trace: {1}", ex.Message, ex.StackTrace);
            throw;
        }
    }

    private DateTime GetDateWithKind(DateTime date)
    {
        if (date.Kind == DateTimeKind.Unspecified)
        {
            return DateTime.SpecifyKind(date, DateTimeKind.Utc);
        }

        return date;
    }
}