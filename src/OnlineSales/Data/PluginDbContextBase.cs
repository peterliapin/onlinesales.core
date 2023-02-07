// <copyright file="PluginDbContextBase.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using Microsoft.EntityFrameworkCore;
using OnlineSales.Interfaces;

namespace OnlineSales.Data;

public abstract class PluginDbContextBase : PgDbContext
{
    protected PluginDbContextBase()
        : base()
    {
    }

    protected PluginDbContextBase(DbContextOptions<PgDbContext> options, IConfiguration configuration, IHttpContextHelper httpContextHelper)
        : base(options, configuration, httpContextHelper)
    {
    }

    protected virtual bool ExcludeBaseEntitiesFromMigrations => true;

    public static T GetPluginDbContext<T>(IServiceScope scope)
        where T : PluginDbContextBase
    {
        var contexts = scope.ServiceProvider.GetServices<PluginDbContextBase>()
                                .Where(s => s.GetType() == typeof(T));
        if (contexts!.Count() > 1)
        {
            throw new PluginDbContextTooManyException(typeof(T));
        }

        var context = contexts.Cast<T>().FirstOrDefault();
        if (context == null)
        {
            throw new PluginDbContextNotFoundException();
        }

        return context;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        if (ExcludeBaseEntitiesFromMigrations)
        {
            var items = modelBuilder.Model.GetEntityTypes();

            foreach (var item in items.Where(item => item.ClrType.Assembly == typeof(PgDbContext).Assembly))
            {
                item.SetIsTableExcludedFromMigrations(true);
            }
        }
    }
}