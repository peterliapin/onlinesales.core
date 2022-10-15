// <copyright file="ApiDbContext.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>
using Microsoft.EntityFrameworkCore;
using OnlineSales.Models;

namespace OnlineSales.Data;

public class ApiDbContext : DbContext
{
    public ApiDbContext(DbContextOptions<ApiDbContext> options)
        : base(options)
    {
        // nothing here yet
    }

    public virtual DbSet<Post>? Posts { get; set; }

    public virtual DbSet<Comment>? Comments { get; set; }

    // protected override void OnModelCreating(ModelBuilder modelBuilder)
    // {
    //    base.OnModelCreating(modelBuilder);

    // nothing here yet
    //
}