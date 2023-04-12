// <copyright file="Content.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Nest;
using OnlineSales.DataAnnotations;

namespace OnlineSales.Entities;

[Table("content")]
[SupportsElastic]
[SupportsChangeLog]
[Index(nameof(Slug), IsUnique = true)]
public class Content : BaseEntity
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
    public string Language { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public string[] Tags { get; set; } = Array.Empty<string>();

    public bool AllowComments { get; set; } = false;

    [Ignore]
    public virtual ICollection<Comment> Comments { get; set; } = new HashSet<Comment>();
}