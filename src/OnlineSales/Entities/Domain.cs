// <copyright file="Domain.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Nest;
using OnlineSales.DataAnnotations;

namespace OnlineSales.Entities;
public enum AccountSyncStatus
{
    NotIntended = 0,
    NotInitialized = 1,
    Successful = 2,
    Failed = 3,
}

[Table("domain")]
[SupportsChangeLog]
[SupportsElastic]
[Index(nameof(Name), IsUnique = true)]

public class Domain : BaseEntityWithIdAndDates
{
    private string name = string.Empty;

    [Required]
    [Searchable]
    public string Name
    {
        get
        {
            return name;
        }

        set
        {
            name = value.ToLower();
        }
    }

    [Searchable]
    public string? Title { get; set; }

    [Searchable]
    public string? Description { get; set; }

    public string? Url { get; set; }

    public string? FaviconUrl { get; set; }

    public bool? HttpCheck { get; set; }

    public bool? Free { get; set; }

    public bool? Disposable { get; set; }

    public bool? CatchAll { get; set; }

    [Nested]
    [Column(TypeName = "jsonb")]
    public List<DnsRecord>? DnsRecords { get; set; }

    public bool? DnsCheck { get; set; }

    public bool? MxCheck { get; set; }

    public int? AccountId { get; set; }

    [Nest.Ignore]
    [JsonIgnore]
    [ForeignKey("AccountId")]
    [DeleteBehavior(DeleteBehavior.Restrict)]
    public virtual Account? Account { get; set; }

    public AccountSyncStatus AccountStatus { get; set; } = AccountSyncStatus.NotIntended;

    [Nest.Ignore]
    [JsonIgnore]
    public virtual ICollection<Contact>? Contacts { get; set; }
}

public class DnsRecord
{
    public string DomainName { get; set; } = string.Empty;

    public string RecordClass { get; set; } = string.Empty;

    public string RecordType { get; set; } = string.Empty;

    public int TimeToLive { get; set; }

    public string Value { get; set; } = string.Empty;
}