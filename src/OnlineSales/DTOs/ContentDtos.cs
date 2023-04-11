// <copyright file="ContentDtos.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel.DataAnnotations;
using CsvHelper.Configuration.Attributes;
using OnlineSales.DataAnnotations;

namespace OnlineSales.DTOs;

public class ContentCreateDto
{
    [Required]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    [Required]
    public string Body { get; set; } = string.Empty;

    public string? CoverImageUrl { get; set; }

    public string? CoverImageAlt { get; set; }

    [Required]
    public string Slug { get; set; } = string.Empty;

    [Required]
    public string Type { get; set; } = string.Empty;

    [Required]
    public string Author { get; set; } = string.Empty;

    [Required]
    public string Language { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public string[] Tags { get; set; } = Array.Empty<string>();

    public bool AllowComments { get; set; } = false;

    [Optional]
    public string? Source { get; set; }
}

public class ContentUpdateDto
{
    [NonEmptyString]
    public string? Title { get; set; }

    [NonEmptyString]
    public string? Description { get; set; }

    [NonEmptyString]
    public string? Body { get; set; }

    public string? CoverImageUrl { get; set; }

    public string? CoverImageAlt { get; set; }

    [NonEmptyString]
    public string? Slug { get; set; }

    [NonEmptyString]
    public string? Type { get; set; }

    public string? Author { get; set; }

    [NonEmptyString]
    public string? Language { get; set; }

    public string? Category { get; set; }

    public string[]? Tags { get; set; }

    public bool? AllowComments { get; set; }

    public string? Source { get; set; }
}

public class ContentDetailsDto : ContentCreateDto
{
    public int Id { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}

public class ContentImportDto : BaseImportDto
{
    [Optional]
    public string? Title { get; set; } = string.Empty;

    [Optional]
    public string? Description { get; set; } = string.Empty;

    [Optional]
    public string? Body { get; set; } = string.Empty;

    [Optional]
    public string? CoverImageUrl { get; set; }

    [Optional]
    public string? CoverImageAlt { get; set; }

    [Optional]
    public string? Slug { get; set; } = string.Empty;

    [Optional]
    public string? Type { get; set; } = string.Empty;

    [Optional]
    public string? Author { get; set; }

    [Optional]
    public string? Language { get; set; }

    [Optional]
    public string? Category { get; set; }

    [Optional]
    public string? Tags { get; set; }

    [Optional]
    public bool? AllowComments { get; set; }
}