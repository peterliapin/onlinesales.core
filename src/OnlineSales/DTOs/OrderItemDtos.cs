// <copyright file="OrderItemDtos.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel.DataAnnotations;
using OnlineSales.DataAnnotations;

namespace OnlineSales.DTOs;

public class OrderItemCreateDto
{
    [Required]
    public int OrderId { get; set; }

    [Required]
    public string ProductName { get; set; } = string.Empty;

    [Required]
    public string LicenseCode { get; set; } = string.Empty;

    [Required]
    public decimal UnitPrice { get; set; } = 0;

    [CurrencyCode]
    [Required]
    public string Currency { get; set; } = string.Empty;
    
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Minimum quantity should be 1")]
    public int Quantity { get; set; } = 0;
}

public class OrderItemUpdateDto
{
    public string? ProductName { get; set; }

    public string? LicenseCode { get; set; }

    public decimal? UnitPrice { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Minimum quantity should be 1")]
    public int? Quantity { get; set; }

    public string? Data { get; set; }
}