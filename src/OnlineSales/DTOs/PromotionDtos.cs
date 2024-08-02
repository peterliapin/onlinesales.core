// <copyright file="PromotionDtos.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel.DataAnnotations;

namespace OnlineSales.DTOs;

public class PromotionCreateDto
{
    [Required]
    public string Code { get; set; } = string.Empty;

    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }
}

public class PromotionUpdateDto
{
    public string? Name { get; set; } = string.Empty;

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }
}

public class PromotionDetailsDto : PromotionCreateDto
{
    public int Id { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}