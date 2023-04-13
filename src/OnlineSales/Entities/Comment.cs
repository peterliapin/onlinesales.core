// <copyright file="Comment.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Nest;
using OnlineSales.DataAnnotations;

namespace OnlineSales.Entities;

public enum CommentStatus
{
    NotApproved = 0,
    Approved = 1,
    Spam = 2,
}

[Table("comment")]
[SupportsElastic]
[SupportsChangeLog]
[SurrogateIdentityAttribute(nameof(Key))]
public class Comment : BaseEntity
{
    [Searchable]
    [Required]
    public string AuthorName { get; set; } = string.Empty;

    [Searchable]
    [EmailAddress]
    public string AuthorEmail { get; set; } = string.Empty;

    [Searchable]
    [Required]
    public string Body { get; set; } = string.Empty;

    public CommentStatus Approved { get; set; } = CommentStatus.NotApproved;

    [Required]
    public int ContentId { get; set; }

    [Ignore]
    [JsonIgnore]
    [ForeignKey("ContentId")]
    public virtual Content? Content { get; set; }

    public int? ParentId { get; set; }

    [Ignore]
    [JsonIgnore]
    [ForeignKey("ParentId")]
    public virtual Comment? Parent { get; set; }

    public string Key { get; set; } = string.Empty;
}