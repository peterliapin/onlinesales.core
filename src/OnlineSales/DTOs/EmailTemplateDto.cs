// <copyright file="EmailTemplateDto.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel.DataAnnotations;

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
    public int GroupId { get; set; }
}

public class EmailTemplateUpdateDto
{
    public string? Name { get; set; }

    public string? Subject { get; set; }

    public string? BodyTemplate { get; set; }

    public string? FromEmail { get; set; }

    public string? FromName { get; set; }

    public int? GroupId { get; set; }
}
