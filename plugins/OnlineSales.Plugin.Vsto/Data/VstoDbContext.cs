// <copyright file="VstoDbContext.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OnlineSales.Data;
using OnlineSales.Plugin.Vsto.Entities;

namespace OnlineSales.Plugin.Vsto.Data;

public class VstoDbContext : PluginDbContextBase
{
    public VstoDbContext()
        : base()
    {
    }

    public VstoDbContext(DbContextOptions<ApiDbContext> options, IConfiguration configuration, IHttpContextHelper httpContextHelper)
        : base(options, configuration, httpContextHelper)
    {
    }

    // Below is the line of code showcasing how a new entity could be added on the plugin level
    public virtual DbSet<VstoUserVersion>? VstoUserVersions { get; set; }
}