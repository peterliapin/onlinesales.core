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

    [Range(1, double.MaxValue, ErrorMessage = "Minimum unit price should be 1")]
    [Required]
    public decimal UnitPrice { get; set; } = 0;

    [CurrencyCode]
    [Required]
    public string Currency { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Minimum quantity should be 1")]
    [Required]
    public int Quantity { get; set; } = 1;

    [Range(1, double.MaxValue, ErrorMessage = "Exchange rate to pay out currency must be greater than 1")]
    [Required]
    public decimal ExchangeRateToPayOutCurrency { get; set; } = 1;

    public string Data { get; set; } = string.Empty;
}

public class OrderItemUpdateDto
{
    [Required]
    public string ProductName { get; set; } = string.Empty;

    [Required]
    public string LicenseCode { get; set; } = string.Empty;

    [Range(1, double.MaxValue, ErrorMessage = "Minimum unit price should be 1")]
    [Required]
    public decimal UnitPrice { get; set; } = 0;

    [Range(1, int.MaxValue, ErrorMessage = "Minimum quantity should be 1")]
    [Required]
    public int Quantity { get; set; }

    public string Data { get; set; } = string.Empty;
}