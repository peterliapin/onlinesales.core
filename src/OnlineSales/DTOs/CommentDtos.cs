// <copyright file="CommentDtos.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel.DataAnnotations;
using CsvHelper.Configuration.Attributes;

namespace OnlineSales.DTOs;

public class CommentCreateDto
{
    public string AuthorName { get; set; } = string.Empty;

    [EmailAddress]
    public string AuthorEmail { get; set; } = string.Empty;

    [Required]
    public string Content { get; set; } = string.Empty;

    [Required]
    public int PostId { get; set; }

    public int? ParentId { get; set; }
}

public class CommentUpdateDto
{
    [Required]
    public string Content { get; set; } = string.Empty;
}

public class CommentDetailsDto : CommentCreateDto
{
    public int Id { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}

public class CommentImportDto : CommentCreateDto
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