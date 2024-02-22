// <copyright file="EmailSyncDbContext.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using EntityFrameworkCore.EncryptColumn.Extension;
using EntityFrameworkCore.EncryptColumn.Interfaces;
using EntityFrameworkCore.EncryptColumn.Util;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Npgsql;
using OnlineSales.Data;
using OnlineSales.Interfaces;
using OnlineSales.Plugin.EmailSync.Entities;

namespace OnlineSales.Plugin.EmailSync.Data;

public class EmailSyncDbContext : PluginDbContextBase
{
    private readonly IEncryptionProvider? encryptionProvider = null;

    public EmailSyncDbContext()
        : base()
    {
    }

    public EmailSyncDbContext(DbContextOptions<PgDbContext> options, IConfiguration configuration, IHttpContextHelper httpContextHelper, NpgsqlDataSource dataSource)
        : base(options, configuration, httpContextHelper, dataSource)
    {
        var key = configuration.GetSection("EmailSync:EncryptionKey").Get<string>();
        if (key == null )
        {
            throw new ArgumentNullException(key);
        }

        encryptionProvider = new GenerateEncryptionProvider(key);
    }

    public DbSet<ImapAccount>? ImapAccounts { get; set; }

    public DbSet<ImapAccountFolder>? ImapAccountFolders { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        if (encryptionProvider != null)
        {
            builder.UseEncryption(encryptionProvider);
        }            
    }
}