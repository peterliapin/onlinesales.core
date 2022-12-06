// <copyright file="PluginDbContextBase.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using Microsoft.EntityFrameworkCore;
using OnlineSales.Entities;

namespace OnlineSales.Data;

public abstract class PluginDbContextBase : ApiDbContext
{
    protected virtual bool ExcludeBaseEntitiesFromMigrations => true;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        if (ExcludeBaseEntitiesFromMigrations)
        {
            var items = modelBuilder.Model.GetEntityTypes();

            foreach (var item in items.Where(item => item.ClrType.Assembly == typeof(ApiDbContext).Assembly))
            {
                item.SetIsTableExcludedFromMigrations(true);
            }
        }
    }
}