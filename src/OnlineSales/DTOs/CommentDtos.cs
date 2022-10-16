// <copyright file="CommentDtos.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>
using System.ComponentModel.DataAnnotations;

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