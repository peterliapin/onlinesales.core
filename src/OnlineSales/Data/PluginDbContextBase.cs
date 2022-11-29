// <copyright file="PluginDbContextBase.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OnlineSales.Entities;

namespace OnlineSales.Data;

public abstract class PluginDbContextBase : ApiDbContext
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Comment>()
            .ToTable("comment", t => t.ExcludeFromMigrations());

        modelBuilder.Entity<Customer>()
            .ToTable("customer", t => t.ExcludeFromMigrations());

        modelBuilder.Entity<Order>()
            .ToTable("order", t => t.ExcludeFromMigrations());

        modelBuilder.Entity<OrderItem>()
            .ToTable("order_item", t => t.ExcludeFromMigrations());

        modelBuilder.Entity<Post>()
            .ToTable("post", t => t.ExcludeFromMigrations());

        modelBuilder.Entity<TaskExecutionLog>()
            .ToTable("task_execution_log", t => t.ExcludeFromMigrations());

        modelBuilder.Entity<EmailGroup>()
            .ToTable("email_groups", t => t.ExcludeFromMigrations());

        modelBuilder.Entity<EmailSchedule>()
            .ToTable("email_schedules", t => t.ExcludeFromMigrations());

        modelBuilder.Entity<EmailTemplate>()
            .ToTable("email_templates", t => t.ExcludeFromMigrations());

        modelBuilder.Entity<CustomerEmailSchedule>()
            .ToTable("customer_email_schedules", t => t.ExcludeFromMigrations());

        modelBuilder.Entity<CustomerEmailLog>()
            .ToTable("customer_email_logs", t => t.ExcludeFromMigrations());
    }
}

