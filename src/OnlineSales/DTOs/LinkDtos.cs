// <copyright file="LinkDtos.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel.DataAnnotations;
using CsvHelper.Configuration.Attributes;

namespace OnlineSales.DTOs;

public class LinkCreateDto
{
    public string? Uid { get; set; }

    [Required]
    public string Destination { get; set; } = string.Empty;

    [Required]
    public string Name { get; set; } = string.Empty;

    [Optional]
    public string? Source { get; set; }
}

public class LinkUpdateDto
{
    public string? Uid { get; set; }

    public string? Destination { get; set; }

    public string? Name { get; set; }

    [Optional]
    public string? Source { get; set; }
}

public class LinkDetailsDto : LinkCreateDto
{
    public int Id { get; set; }
    
    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}

public class LinkImportDto : BaseImportDto
{
    [Optional]
    public string? Uid { get; set; }

    [Optional]
    public string? Destination { get; set; }

    [Optional]
    public string? Name { get; set; }
}

