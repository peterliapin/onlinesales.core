// <copyright file="OrderDtos.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel.DataAnnotations;

namespace OnlineSales.DTOs;

public class OrderCreateDto
{
    [Required]
    public int CustomerId { get; set; }

    [Required]
    public string RefNo { get; set; } = string.Empty;

    public string? OrderNumber { get; set; } = string.Empty;

    [Required]
    public decimal Total { get; set; } = 0;

    [Required]
    public string Currency { get; set; } = string.Empty;

    [Required]
    public decimal CurrencyTotal { get; set; } = 0;

    public int Quantity { get; set; } = 0;

    public string? AffiliateName { get; set; }

    public bool TestOrder { get; set; } = false;

    public string? Data { get; set; }
}

public class OrderUpdateDto
{
    [Required]
    public decimal Total { get; set; } = 0;

    [Required]
    public string Currency { get; set; } = string.Empty;

    [Required]
    public decimal CurrencyTotal { get; set; } = 0;

    public int Quantity { get; set; }

    public string? Data { get; set; }
}
