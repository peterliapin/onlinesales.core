// <copyright file="ImapAccount.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using EntityFrameworkCore.EncryptColumn.Attribute;
using OnlineSales.DataAnnotations;
using OnlineSales.Entities;

namespace OnlineSales.Plugin.EmailSync.Entities;

[Table("imap_account")]
[SupportsChangeLog]
public class ImapAccount : BaseEntity
{
    public string Host { get; set; } = string.Empty;

    public string UserName { get; set; } = string.Empty;

    [EncryptColumn]
    public string Password { get; set; } = string.Empty;

    public int Port { get; set; }

    public bool UseSsl { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [JsonIgnore]
    [ForeignKey("UserId")]
    public virtual User? User { get; set; }
}