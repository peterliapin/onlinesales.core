// <copyright file="PgDbContext.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Reflection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql;
using OnlineSales.Configuration;
using OnlineSales.DataAnnotations;
using OnlineSales.Entities;
using OnlineSales.Helpers;
using OnlineSales.Infrastructure;
using OnlineSales.Interfaces;

namespace OnlineSales.Data;

public class PgDbContext : IdentityDbContext<User>
{
    public readonly IConfiguration Configuration;

    private readonly IHttpContextHelper? httpContextHelper;

    /// <summary>
    /// Initializes a new instance of the <see cref="PgDbContext"/> class.
    /// Constructor with no parameters and manual configuration building is required for the case when you would like to use PgDbContext as a base class for a new context (let's say in a plugin).
    /// </summary>
    public PgDbContext()
    {
        try
        {
            Console.WriteLine("Initializing PgDbContext...");

            Configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", false)
                .AddEnvironmentVariables()
                .AddUserSecrets(typeof(Program).Assembly)
                .Build();

            Console.WriteLine("PgDbContext initialized");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Failed to create PgDbContext. Error: {0}, Stack Trace: {1}", ex.Message, ex.StackTrace);
            throw;
        }
    }

    public PgDbContext(DbContextOptions<PgDbContext> options, IConfiguration configuration, IHttpContextHelper httpContextHelper)
        : base(options)
    {
        Configuration = configuration;
        this.httpContextHelper = httpContextHelper;
    }

    public bool IsImportRequest { get; set; }

    public virtual DbSet<Content>? Content { get; set; }

    public virtual DbSet<Comment>? Comments { get; set; }

    public virtual DbSet<Contact>? Contacts { get; set; }

    public virtual DbSet<Order>? Orders { get; set; }

    public virtual DbSet<OrderItem>? OrderItems { get; set; }

    public virtual DbSet<TaskExecutionLog>? TaskExecutionLogs { get; set; }

    public virtual DbSet<Media>? Media { get; set; }

    public virtual DbSet<Entities.File>? Files { get; set; }

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

    public virtual DbSet<Account>? Accounts { get; set; }

    public virtual DbSet<Unsubscribe>? Unsubscribes { get; set; }

    public virtual DbSet<Deal>? Deals { get; set; }

    public virtual DbSet<DealPipeline>? DealPipelines { get; set; }

    public virtual DbSet<DealPipelineStage>? DealPipelineStages { get; set; }

    public virtual DbSet<Promotion>? Promotions { get; set; }

    public virtual DbSet<Discount>? Discounts { get; set; }

    public virtual DbSet<MailServer>? MailServers { get; set; }

    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        var result = 0;
        var changes = new Dictionary<EntityEntry, ChangeLog>();

        var entries = ChangeTracker
           .Entries()
           .Where(e => e.Entity is BaseEntityWithId && (
                   e.State == EntityState.Added
                   || e.State == EntityState.Modified
                   || e.State == EntityState.Deleted));

        var currentUserId = await httpContextHelper!.GetCurrentUserIdAsync();
        var userIpAddress = httpContextHelper!.IpAddressV4;
        var userAgent = httpContextHelper!.UserAgent;

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
                        createdByEntity.CreatedById = currentUserId;
                        createdByEntity.CreatedByIp = string.IsNullOrEmpty(createdByEntity.CreatedByIp) ? userIpAddress : createdByEntity.CreatedByIp;
                        createdByEntity.CreatedByUserAgent = string.IsNullOrEmpty(createdByEntity.CreatedByUserAgent) ? userAgent : createdByEntity.CreatedByUserAgent;
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
                        updatedByEntity.UpdatedById = currentUserId;
                        updatedByEntity.UpdatedByIp = IsImportRequest && !string.IsNullOrEmpty(updatedByEntity.UpdatedByIp) ? updatedByEntity.UpdatedByIp : userIpAddress;
                        updatedByEntity.UpdatedByUserAgent = IsImportRequest && !string.IsNullOrEmpty(updatedByEntity.UpdatedByUserAgent) ? updatedByEntity.UpdatedByUserAgent : userAgent;
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
            Console.WriteLine("Configuring PgDbContext...");

            var postgresConfig = Configuration.GetSection("Postgres").Get<PostgresConfig>();

            if (postgresConfig == null)
            {
                throw new MissingConfigurationException("Postgres configuration is mandatory.");
            }

            var dataSourceBuilder = new NpgsqlDataSourceBuilder(postgresConfig.ConnectionString);
            dataSourceBuilder.EnableDynamicJson();
            var dataSource = dataSourceBuilder.Build();

            optionsBuilder.UseNpgsql(
                dataSource,
                b => b.MigrationsHistoryTable("_migrations"))
                        .UseSnakeCaseNamingConvention()
                        .ReplaceService<IMigrationsSqlGenerator, CustomSqlServerMigrationsSqlGenerator>();

            Console.WriteLine("PgDbContext successfully configured");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Failed to configure PgDbContext. Error: {0}, Stack Trace: {1}", ex.Message, ex.StackTrace);
            throw;
        }
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Override default AspNet Identity table names
        builder.Entity<User>(entity => { entity.ToTable(name: "users"); });
        builder.Entity<IdentityRole>(entity => { entity.ToTable(name: "roles"); });
        builder.Entity<IdentityUserRole<string>>(entity => { entity.ToTable("user_roles"); });
        builder.Entity<IdentityUserClaim<string>>(entity => { entity.ToTable("user_claims"); });
        builder.Entity<IdentityUserLogin<string>>(entity => { entity.ToTable("user_logins"); });
        builder.Entity<IdentityUserToken<string>>(entity => { entity.ToTable("user_tokens"); });
        builder.Entity<IdentityRoleClaim<string>>(entity => { entity.ToTable("role_claims"); });

        builder.Entity<User>().Property(u => u.Data).HasColumnType("jsonb");
    }

    private DateTime GetDateWithKind(DateTime date)
    {
        if (date.Kind == DateTimeKind.Unspecified /*|| date.Kind == DateTimeKind.Local*/)
        {
            return DateTime.SpecifyKind(date, DateTimeKind.Utc);
        }

        return date;
    }
}