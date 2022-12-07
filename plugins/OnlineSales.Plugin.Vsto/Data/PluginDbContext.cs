// <copyright file="PluginDbContext.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OnlineSales.Data;
using OnlineSales.Plugin.Vsto.Entities;

namespace OnlineSales.Plugin.Vsto.Data;

public class PluginDbContext : PluginDbContextBase
{
    public PluginDbContext(DbContextOptions<ApiDbContext> options, IConfiguration configuration, IHttpContextHelper httpContextHelper)
        : base(options, configuration, httpContextHelper)
    {
    }

    // Below is the line of code showcasing how a new entity could be added on the plugin level
    public virtual DbSet<VstoUserVersion>? VstoUserVersions { get; set; }
}