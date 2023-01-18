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
}

public class DomainUpdateDto
{
    public bool? Shared { get; set; }

    public bool? Disposable { get; set; }

    public string? Name { get; set; }
}

public class DomainDetailsDto : DomainCreateDto
{
    public int Id { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}

public class DomainImportDto : DomainCreateDto
{
    [Optional]
    public int? Id { get; set; }

    [Optional]
    public DateTime? CreatedAt { get; set; }

    [Optional]
    public DateTime? UpdatedAt { get; set; }
}