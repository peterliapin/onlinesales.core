// <copyright file="Comment.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineSales.Models;

public enum CommentStatus
{
    NOTAPPROVED = 0,
    APPROVED = 1,
    SPAM = 2,
}

public class Comment : BaseEntity
{   
    public string AuthorName { get; set; } = string.Empty;

    public string AuthorEmail { get; set; } = string.Empty;

    [Required]
    public string AuthorIP { get; set; } = string.Empty;

    [Required]
    public string AuthorAgent { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    [Required]
    public string Content { get; set; } = string.Empty;

    public CommentStatus Approved { get; set; } = CommentStatus.NOTAPPROVED;

    public int PostId { get; set; }

    [Required]
    [ForeignKey("PostId")]
    public virtual Post? Post { get; set; }

    public int ParentId { get; set; }

    [ForeignKey("ParentId")]
    public virtual Comment? Parent { get; set; }
}