// <copyright file="WaveServicePluginDbContext.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OnlineSales.Data;
using OnlineSales.Interfaces;
using OnlineSales.Plugin.WaveService.Data.Seed;

namespace OnlineSales.Plugin.WaveService.Data;

public class WaveServicePluginDbContext : PluginDbContextBase
{
    public WaveServicePluginDbContext()
        : base()
    {
    }

    public WaveServicePluginDbContext(DbContextOptions<PgDbContext> options, IConfiguration configuration, IHttpContextHelper httpContextHelper)
        : base(options, configuration, httpContextHelper)
    {
    }

    protected override bool ExcludeBaseEntitiesFromMigrations => true;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        EmailTemplateData.Seed(modelBuilder);

        base.OnModelCreating(modelBuilder);
    }
}
