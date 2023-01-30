// <copyright file="Domain.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using OnlineSales.DataAnnotations;

namespace OnlineSales.Entities;

[Table("domain")]
[SupportsChangeLog]
[SupportsElasticSearch]
[Index(nameof(Name), IsUnique = true)]
public class Domain : BaseEntityWithIdAndDates
{
    [Required]
    public string Name { get; set; } = string.Empty;

    public string? Title { get; set; }

    public string? Description { get; set; }

    public string? Url { get; set; }

    public bool? HttpCheck { get; set; }

    public bool? Free { get; set; }

    public bool? Disposable { get; set; }

    public bool? CatchAll { get; set; }

    [Column(TypeName = "jsonb")]
    public List<DnsRecord>? DnsRecords { get; set; }

    public bool? DnsCheck { get; set; }
}

public class DnsRecord
{
    public string DomainName { get; set; } = string.Empty;

    public string RecordClass { get; set; } = string.Empty;

    public string RecordType { get; set; } = string.Empty;

    public int TimeToLive { get; set; }

    public string Value { get; set; } = string.Empty;
}