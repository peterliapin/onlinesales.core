// <copyright file="DealPipelineStageDto.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel.DataAnnotations;
using CsvHelper.Configuration.Attributes;
using OnlineSales.DataAnnotations;

namespace OnlineSales.DTOs;

public class DealPipelineStageCreateDto
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public int DealPipelineId { get; set; }

    [Required]
    public int Order { get; set; }
}

public class DealPipelineStageUpdateDto
{
    [MinLength(1)]
    public string? Name { get; set; }

    public int? Order { get; set; }
}

public class DealPipelineStageDetailsDto : DealPipelineStageCreateDto
{
    public int Id { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    [Ignore]
    public DealPipelineDetailsDto? DealPipeline { get; set; }
}
