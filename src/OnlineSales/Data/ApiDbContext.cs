// <copyright file="ApiDbContext.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
using OnlineSales.Entities;
using OnlineSales.Interfaces;

namespace OnlineSales.Data;

public class ApiDbContext : DbContext
{
    private readonly IHttpContextHelper httpContextHelper;

    public ApiDbContext(DbContextOptions<ApiDbContext> options, IHttpContextHelper httpContextHelper)
        : base(options)
    {
        this.httpContextHelper = httpContextHelper;
        // nothing here yet
    }

    public virtual DbSet<Post>? Posts { get; set; }

    public virtual DbSet<Comment>? Comments { get; set; }

    public virtual DbSet<Customer>? Customers { get; set; }

    public virtual DbSet<Order>? Orders { get; set; }

    public virtual DbSet<OrderItem>? OrderItems { get; set; }

    public virtual DbSet<TaskExecutionLog>? TaskExecutionLogs { get; set; }

    public virtual DbSet<Image>? Images { get; set; }

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
                ((BaseEntity)entityEntry.Entity).UpdatedByIP = httpContextHelper.IpAddress;
                ((BaseEntity)entityEntry.Entity).UpdatedByUserAgent = httpContextHelper.UserAgent;
            }

            if (entityEntry.State == EntityState.Added)
            {
                ((BaseEntity)entityEntry.Entity).CreatedAt = DateTime.UtcNow;
                ((BaseEntity)entityEntry.Entity).CreatedByIP = httpContextHelper.IpAddress;
                ((BaseEntity)entityEntry.Entity).CreatedByUserAgent = httpContextHelper.UserAgent;
            }
        }

        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    } 
}