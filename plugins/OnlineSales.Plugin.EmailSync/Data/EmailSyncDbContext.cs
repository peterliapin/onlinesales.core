// <copyright file="EmailSyncDbContext.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using EntityFrameworkCore.EncryptColumn.Extension;
using EntityFrameworkCore.EncryptColumn.Interfaces;
using EntityFrameworkCore.EncryptColumn.Util;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OnlineSales.Data;
using OnlineSales.Interfaces;
using OnlineSales.Plugin.EmailSync.Entities;

namespace OnlineSales.Plugin.EmailSync.Data;

public class EmailSyncDbContext : PluginDbContextBase
{
    private readonly IEncryptionProvider encryptionProvider;

    private readonly string key = "fTjWnZr4u7x!A%D*";

    public EmailSyncDbContext()
        : base()
    {
        encryptionProvider = new GenerateEncryptionProvider(key);
    }

    public EmailSyncDbContext(DbContextOptions<PgDbContext> options, IConfiguration configuration, IHttpContextHelper httpContextHelper)
        : base(options, configuration, httpContextHelper)
    {
        encryptionProvider = new GenerateEncryptionProvider(key);
    }

    public DbSet<ImapAccount>? ImapAccounts { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.UseEncryption(encryptionProvider);
    }
}