// <copyright file="EmailTemplateDtos.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel.DataAnnotations;
using CsvHelper.Configuration.Attributes;

namespace OnlineSales.DTOs;

public class EmailTemplateCreateDto
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Subject { get; set; } = string.Empty;

    [Required]
    public string BodyTemplate { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string FromEmail { get; set; } = string.Empty;

    [Required]
    public string FromName { get; set; } = string.Empty;

    [Required]
    public string Language { get; set; } = string.Empty;

    [Required]
    public int EmailGroupId { get; set; }
}

public class EmailTemplateUpdateDto
{
    [MinLength(1)]
    public string? Name { get; set; }

    [MinLength(1)]
    public string? Subject { get; set; }

    [MinLength(1)]
    public string? BodyTemplate { get; set; }

    [EmailAddress]
    public string? FromEmail { get; set; }

    [MinLength(1)]
    public string? FromName { get; set; }

    public int? EmailGroupId { get; set; }
}

public class EmailTemplateDetailsDto : EmailTemplateCreateDto
{
    public int Id { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    [Ignore]
    public EmailGroupDetailsDto? EmailGroup { get; set; }
}