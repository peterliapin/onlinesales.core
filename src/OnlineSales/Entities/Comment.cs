// <copyright file="Comment.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace OnlineSales.Entities;

public enum CommentStatus
{
    NotApproved = 0,
    Approved = 1,
    Spam = 2,
}

[Table("comment")]
public class Comment : BaseEntity
{   
    public string AuthorName { get; set; } = string.Empty;

    [EmailAddress]
    public string AuthorEmail { get; set; } = string.Empty;
    
    [Required]
    public string Content { get; set; } = string.Empty;

    [JsonIgnore]
    public CommentStatus Approved { get; set; } = CommentStatus.NotApproved;

    [Required]
    public int PostId { get; set; }
    
    [JsonIgnore]
    [ForeignKey("PostId")]    
    public virtual Post? Post { get; set; }

    public int? ParentId { get; set; }

    [JsonIgnore]
    [ForeignKey("ParentId")]
    public virtual Comment? Parent { get; set; }
}