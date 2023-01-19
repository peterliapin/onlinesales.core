// <copyright file="DomainDtos.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CsvHelper.Configuration.Attributes;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace OnlineSales.DTOs;

public class DomainCreateDto
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
    public string? DnsRecords { get; set; }

    public bool? DnsCheck { get; set; }
}

public class DomainUpdateDto
{
    public string? Name { get; set; }

    public string? Title { get; set; }

    public string? Description { get; set; }

    public string? Url { get; set; }

    public bool? HttpCheck { get; set; }

    public bool? Free { get; set; }

    public bool? Disposable { get; set; }

    public bool? CatchAll { get; set; }

    [Column(TypeName = "jsonb")]
    public string? DnsRecords { get; set; }

    public bool? DnsCheck { get; set; }
}

public class DomainDetailsDto : DomainCreateDto
{
    public int Id { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}

public class DomainImportDto
{
    [Optional]
    public int? Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    [Optional]
    public string? Title { get; set; }

    [Optional]
    public string? Description { get; set; }

    [Optional]
    public string? Url { get; set; }

    [Optional]
    public bool? HttpCheck { get; set; }

    [Optional]
    public bool? Free { get; set; }

    [Optional]
    public bool? Disposable { get; set; }

    [Optional]
    public bool? CatchAll { get; set; }

    [Optional]
    [Column(TypeName = "jsonb")]
    public string? DnsRecords { get; set; }

    [Optional]
    public bool? DnsCheck { get; set; }

    [Optional]
    public DateTime? CreatedAt { get; set; }

    [Optional]
    public DateTime? UpdatedAt { get; set; }
}