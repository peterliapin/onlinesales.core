// <copyright file="EmailGroupDto.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using CsvHelper.Configuration.Attributes;
using OnlineSales.DataAnnotations;
using OnlineSales.Entities;
using static OnlineSales.Helpers.JsonHelper;

namespace OnlineSales.DTOs;

public class EmailGroupCreateDto
{
    [Required]
    public string Name { get; set; } = string.Empty;
}

public class EmailGroupUpdateDto
{
    [MinLength(1)]
    public string? Name { get; set; }
}

public class EmailGroupDetailsDto : EmailGroupCreateDto
{
    public int Id { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    [Ignore]
    public List<EmailTemplateDetailsDto>? EmailTemplates { get; set; }
}
