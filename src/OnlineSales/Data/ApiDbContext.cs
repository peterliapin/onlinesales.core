// <copyright file="ApiDbContext.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using OnlineSales.Configuration;
using OnlineSales.DataAnnotations;
using OnlineSales.Entities;
using OnlineSales.Helpers;
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

    public virtual DbSet<Contact>? Contacts { get; set; }

    public virtual DbSet<Order>? Orders { get; set; }

    public virtual DbSet<OrderItem>? OrderItems { get; set; }

    public virtual DbSet<TaskExecutionLog>? TaskExecutionLogs { get; set; }

    public virtual DbSet<Image>? Images { get; set; }

    public virtual DbSet<EmailGroup>? EmailGroups { get; set; }

    public virtual DbSet<EmailSchedule>? EmailSchedules { get; set; }

    public virtual DbSet<EmailTemplate>? EmailTemplates { get; set; }

    public virtual DbSet<ContactEmailSchedule>? ContactEmailSchedules { get; set; }

    public virtual DbSet<EmailLog>? EmailLogs { get; set; }

    public virtual DbSet<IpDetails>? IpDetails { get; set; }

    public virtual DbSet<ChangeLog>? ChangeLogs { get; set; }

    public virtual DbSet<ChangeLogTaskLog>? ChangeLogTaskLogs { get; set; }

    public virtual DbSet<Link>? Links { get; set; }

    public virtual DbSet<LinkLog>? LinkLogs { get; set; }

    public virtual DbSet<Domain>? Domains { get; set; }

    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        int result = 0;
        var changes = new Dictionary<EntityEntry, ChangeLog>();

        var entries = ChangeTracker
           .Entries()
           .Where(e => e.Entity is BaseEntityWithId && (
                   e.State == EntityState.Added
                   || e.State == EntityState.Modified
                   || e.State == EntityState.Deleted));

        if (entries.Any())
        {
            foreach (var entityEntry in entries)
            {
                if (entityEntry.State == EntityState.Added)
                {
                    var createdAtEntity = entityEntry.Entity as IHasCreatedAt;

                    if (createdAtEntity is not null)
                    {
                        createdAtEntity.CreatedAt = createdAtEntity.CreatedAt == DateTime.MinValue ? DateTime.UtcNow : GetDateWithKind(createdAtEntity.CreatedAt);
                    }

                    var createdByEntity = entityEntry.Entity as IHasCreatedBy;

                    if (createdByEntity is not null)
                    {
                        createdByEntity.CreatedByIp = string.IsNullOrEmpty(createdByEntity.CreatedByIp) ? httpContextHelper!.IpAddress : createdByEntity.CreatedByIp;
                        createdByEntity.CreatedByUserAgent = string.IsNullOrEmpty(createdByEntity.CreatedByUserAgent) ? httpContextHelper!.UserAgent : createdByEntity.CreatedByUserAgent;
                    }
                }

                if (entityEntry.State == EntityState.Modified)
                {
                    var updatedAtEntity = entityEntry.Entity as IHasUpdatedAt;

                    if (updatedAtEntity is not null)
                    {
                        updatedAtEntity.UpdatedAt = IsImportRequest && updatedAtEntity.UpdatedAt is not null ? GetDateWithKind(updatedAtEntity.UpdatedAt.Value) : DateTime.UtcNow;
                    }

                    var updatedByEntity = entityEntry.Entity as IHasUpdatedBy;

                    if (updatedByEntity is not null)
                    {
                        updatedByEntity.UpdatedByIp = IsImportRequest && !string.IsNullOrEmpty(updatedByEntity.UpdatedByIp) ? updatedByEntity.UpdatedByIp : httpContextHelper!.IpAddress;
                        updatedByEntity.UpdatedByUserAgent = IsImportRequest && !string.IsNullOrEmpty(updatedByEntity.UpdatedByUserAgent) ? updatedByEntity.UpdatedByUserAgent : httpContextHelper!.UserAgent;
                    }
                }

                var entityType = entityEntry.Entity.GetType();

                if (entityType!.GetCustomAttributes<SupportsChangeLogAttribute>().Any())
                {
                    // save entity state as it is before SaveChanges call
                    changes[entityEntry] = new ChangeLog
                    {
                        ObjectType = entityEntry.Entity.GetType().Name,
                        EntityState = entityEntry.State,
                        CreatedAt = DateTime.UtcNow,
                    };
                }
            }
        }

        if (changes.Count > 0)
        {
            // save original records and obtain ids (to preserve ids in change_log)
            result = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);

            foreach (var change in changes)
            {
                // save object id which we only recieve after SaveChanges (for new records)
                change.Value.ObjectId = ((BaseEntityWithId)change.Key.Entity).Id;
                change.Value.Data = JsonHelper.Serialize(change.Key.Entity);
            }

            ChangeLogs!.AddRange(changes.Values);
        }

        return result + await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
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