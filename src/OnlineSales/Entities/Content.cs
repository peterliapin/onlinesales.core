// <copyright file="Content.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Nest;
using OnlineSales.DataAnnotations;

namespace OnlineSales.Entities;

[Table("content")]
[SupportsElastic]
[SupportsChangeLog]
[Index(nameof(Slug), IsUnique = true)]
public class Content : BaseEntity, ICommentable
{
    [Searchable]
    [Required]
    public string Title { get; set; } = string.Empty;

    [Searchable]
    [Required]
    public string Description { get; set; } = string.Empty;

    [Searchable]
    [Required]
    public string Body { get; set; } = string.Empty;

    public string CoverImageUrl { get; set; } = string.Empty;

    public string CoverImageAlt { get; set; } = string.Empty;

    [Required]
    public string Slug { get; set; } = string.Empty;

    [Required]
    public string Type { get; set; } = string.Empty;

    [Searchable]
    [Required]
    public string Author { get; set; } = string.Empty;

    [Searchable]
    [Required]
    [LanguageCode]
    public string Language { get; set; } = string.Empty;

    [Searchable]
    public string Category { get; set; } = string.Empty;

    [Searchable]
    public string[] Tags { get; set; } = Array.Empty<string>();

    public bool AllowComments { get; set; } = false;

    public DateTime? PublishedAt { get; set; } = DateTime.UtcNow;

    public static string GetCommentableType()
    {
        return "Content";
    }
}