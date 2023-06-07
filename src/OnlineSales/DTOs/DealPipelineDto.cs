// <copyright file="DealPipelineDto.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel.DataAnnotations;
using CsvHelper.Configuration.Attributes;
using OnlineSales.DataAnnotations;

namespace OnlineSales.DTOs;

public class DealPipelineCreateDto
{
    [Required]
    public string Name { get; set; } = string.Empty;
}

public class DealPipelineUpdateDto
{
    [MinLength(1)]
    public string? Name { get; set; }
}

public class DealPipelineDetailsDto : DealPipelineCreateDto
{
    public int Id { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    [Ignore]
    public List<DealPipelineStageDetailsDto>? PipelineStages { get; set; }
}
