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
}

public class LinkUpdateDto
{
    public string? Uid { get; set; }

    public string? Destination { get; set; }

    public string? Name { get; set; }
}

public class LinkDetailsDto : LinkCreateDto
{
    public int Id { get; set; }
    
    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}

public class LinkImportDto : LinkCreateDto
{
    [Optional]
    public int? Id { get; set; }

    [Optional]
    public DateTime? CreatedAt { get; set; }

    [Optional]
    public DateTime? UpdatedAt { get; set; }

    [Optional]
    public string? CreatedByIp { get; set; }

    [Optional]
    public string? CreatedByUserAgent { get; set; }

    [Optional]
    public string? UpdatedByIp { get; set; }

    [Optional]
    public string? UpdatedByUserAgent { get; set; }
}

