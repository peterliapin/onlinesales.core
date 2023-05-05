// <copyright file="CommentDtos.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel.DataAnnotations;
using CsvHelper.Configuration.Attributes;
using OnlineSales.DataAnnotations;
using OnlineSales.Entities;

namespace OnlineSales.DTOs;

public class CommentCreateDto
{
    private string authorEmail = string.Empty;

    [Required]
    [EmailAddress]
    public string AuthorEmail
    {
        get
        {
            return this.authorEmail;
        }

        set
        {
            this.authorEmail = value.ToLower();
        }
    }

    public string AuthorName { get; set; } = string.Empty;

    [Required]
    public string Body { get; set; } = string.Empty;

    [Required]
    public int ContentId { get; set; }

    public int? ContactId { get; set; }

    public int? ParentId { get; set; }    

    [Optional]
    public string? Source { get; set; }
}

public class CommentUpdateDto
{
    [Required]
    public string Body { get; set; } = string.Empty;
}

public class AnonymousCommentDetailsDto
{
    public int Id { get; set; }

    public int? ParentId { get; set; }

    public string AuthorName { get; set; } = string.Empty;

    public string Body { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int ContentId { get; set; }

    public string AvatarUrl { get; set; } = string.Empty;

    [Ignore]
    public ContentDetailsDto? Content { get; set; }

    [Ignore]
    public CommentDetailsDto? Parent { get; set; }
}

public class CommentDetailsDto : AnonymousCommentDetailsDto
{
    public string AuthorEmail { get; set; } = string.Empty;

    public int? ContactId { get; set; }

    public string? Source { get; set; }

    [Ignore]

    public ContactDetailsDto? Contact { get; set; }
}

public class CommentImportDto : BaseImportDto
{
    private string? authorEmail;

    [Optional]
    public int? ContactId { get; set; }

    [Optional]
    public string? AuthorName { get; set; }

    [Optional]
    [EmailAddress]
    [SurrogateForeignKey(typeof(Contact), "Email", "ContactId")]
    public string? AuthorEmail
    {
        get
        {
            return this.authorEmail;
        }

        set
        {
            this.authorEmail = string.IsNullOrEmpty(value) ? null : value.ToLower();
        }
    }

    [Optional]
    public string Body { get; set; } = string.Empty;

    [Optional]
    public CommentStatus? Status { get; set; }

    [Optional]
    public string? Language { get; set; }

    [Optional]
    public int? ContentId { get; set; }

    [Optional]
    [SurrogateForeignKey(typeof(Content), "Slug", "ContentId")]
    public string? ContentSlug { get; set; }

    [Optional]
    public int? ParentId { get; set; }

    [Optional]
    public string? Key { get; set; }

    [Optional]
    [SurrogateForeignKey(typeof(Comment), "Key", "ParentId")]
    public string? ParentKey { get; set; }
}