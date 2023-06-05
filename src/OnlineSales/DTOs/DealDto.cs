// <copyright file="DealDto.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel.DataAnnotations;
using CsvHelper.Configuration.Attributes;

namespace OnlineSales.DTOs;

public class DealBaseDto
{
    public int? AccountId { get; set; }

    [Required]
    public int DealPipelineId { get; set; }

    public decimal DealValue { get; set; }

    [Required]
    public string DealCurrency { get; set; } = string.Empty;

    public DateTime? ExpectedCloseDate { get; set; }

    public DateTime? ActualCloseDate { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;
}

public class DealCreateDto : DealBaseDto
{
    public HashSet<int> ContactIds { get; set; } = new HashSet<int>();
}

public class DealUpdateDto
{
    public int? AccountId { get; set; }

    public int? DealPipelineId { get; set; }

    public HashSet<int>? ContactIds { get; set; }

    public decimal? DealValue { get; set; }

    [MinLength(1)]
    public string? DealCurrency { get; set; }

    public DateTime? ExpectedCloseDate { get; set; }

    public DateTime? ActualCloseDate { get; set; }

    public string? UserId { get; set; }
}

public class DealDetailsDto : DealBaseDto
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
