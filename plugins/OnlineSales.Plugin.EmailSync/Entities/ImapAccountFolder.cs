// <copyright file="ImapAccountFolder.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using OnlineSales.DataAnnotations;
using OnlineSales.Entities;

namespace OnlineSales.Plugin.EmailSync.Entities;

[Table("imap_account_folder")]
[SupportsChangeLog]
public class ImapAccountFolder : BaseEntity
{
    public string FullName { get; set; } = string.Empty;

    public int LastUid { get; set; }

    [Required]
    public int ImapAccountId { get; set; }

    [JsonIgnore]
    [ForeignKey("ImapAccountId")]
    public virtual ImapAccount? ImapAccount { get; set; }
}