// <copyright file="DealPipelineDtos.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel.DataAnnotations;
using CsvHelper.Configuration.Attributes;

namespace OnlineSales.DTOs;

public class DealPipelineCreateDto
{
    [Required]
    public string Name { get; set; } = string.Empty;
}

public class DealPipelineUpdateDto
{
    [Required]
    public string Name { get; set; } = string.Empty;
}

public class DealPipelineDetailsDto : DealPipelineCreateDto
{
    public int Id { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    [Ignore]
    public List<DealPipelineStageDetailsDto>? PipelineStages { get; set; }
}