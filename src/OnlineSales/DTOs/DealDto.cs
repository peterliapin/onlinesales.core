// <copyright file="DealDto.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel.DataAnnotations;
using CsvHelper.Configuration.Attributes;
using OnlineSales.DataAnnotations;

namespace OnlineSales.DTOs;

public class DealCreateDto
{
    [Required]
    public int AccountId { get; set; }

    [Required]
    public int DealPipelineId { get; set; }

    public decimal DealMoney { get; set; }

    [Required]
    public string Currency { get; set; } = string.Empty;

    [Required]
    public DateTime ExpectedCloseDate { get; set; }

    public DateTime? ActualCloseDate { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;
}

public class DealUpdateDto
{
    public int? AccountId { get; set; }

    public int? DealPipelineId { get; set; }

    public decimal? DealMoney { get; set; }

    [MinLength(1)]
    public string? Currency { get; set; }

    public DateTime? ExpectedCloseDate { get; set; }

    public DateTime? ActualCloseDate { get; set; }

    public string? UserId { get; set; }
}

public class DealDetailsDto : DealCreateDto
{
    public int Id { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    [Ignore]
    public AccountDetailsDto? Account { get; set; }

    [Ignore]
    public DealPipelineDetailsDto? DealPipeline { get; set; }

    [Ignore]
    public PipelineStageDetailsDto? PipelineStage { get; set; }

    [Ignore]
    public List<ContactDetailsDto>? Contacts { get; set; }
}
