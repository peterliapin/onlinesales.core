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
        var items = modelBuilder.Model.GetEntityTypes();

#pragma warning disable S3267
        foreach (var item in items)
        {
            if (item.ClrType.IsSubclassOf(typeof(BaseEntity)))
            {
                item.SetIsTableExcludedFromMigrations(true);
            }
        }
#pragma warning restore S3267
       
    }
}

