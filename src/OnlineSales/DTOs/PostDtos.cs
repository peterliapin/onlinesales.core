// <copyright file="PostDtos.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel.DataAnnotations;

namespace OnlineSales.DTOs;

public class PostCreateDto
{
    [Required]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    [Required]
    public string Content { get; set; } = string.Empty;

    public string CoverImageUrl { get; set; } = string.Empty;

    public string CoverImageAlt { get; set; } = string.Empty;

    [Required]
    public string Slug { get; set; } = string.Empty;

    [Required]
    public string Template { get; set; } = string.Empty;

    [Required]
    public string Author { get; set; } = string.Empty;

    [Required]
    public string Language { get; set; } = string.Empty;

    public string Categories { get; set; } = string.Empty;

    public string Tags { get; set; } = string.Empty;

    public bool AllowComments { get; set; } = false;
}

public class PostUpdateDto
{
    public string? Title { get; set; }

    public string? Description { get; set; }

    public string? Content { get; set; }

    public string? CoverImageUrl { get; set; }

    public string? CoverImageAlt { get; set; }

    public string? Slug { get; set; }

    public string? Template { get; set; }

    public string? Author { get; set; }

    public string? Language { get; set; }

    public string? Categories { get; set; }

    public string? Tags { get; set; }

    public bool? AllowComments { get; set; }
}