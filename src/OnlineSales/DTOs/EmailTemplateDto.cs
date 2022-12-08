// <copyright file="EmailTemplateDto.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel.DataAnnotations;
using OnlineSales.Entities;

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
    public string FromEmail { get; set; } = string.Empty;

    [Required]
    public string FromName { get; set; } = string.Empty;

    [Required]
    public int GroupId { get; set; }
}

public class EmailTemplateUpdateDto
{
    public string? Name { get; set; } = string.Empty;

    public string? Subject { get; set; } = string.Empty;

    public string? BodyTemplate { get; set; } = string.Empty;

    public string? FromEmail { get; set; } = string.Empty;

    public string? FromName { get; set; } = string.Empty;

    public int? GroupId { get; set; }
}
