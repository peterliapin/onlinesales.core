// <copyright file="OrderItemDtos.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel.DataAnnotations;

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
    public decimal Total { get; set; } = 0;

    [Required]
    public string Currency { get; set; } = string.Empty;

    [Required]
    public decimal CurrencyTotal { get; set; } = 0;

    public int Quantity { get; set; }
}

public class OrderItemUpdateDto
{
    public string ProductName { get; set; } = string.Empty;

    public string LicenseCode { get; set; } = string.Empty;
    
    public decimal Total { get; set; } = 0;
   
    public string Currency { get; set; } = string.Empty;
    
    public decimal CurrencyTotal { get; set; } = 0;

    public int Quantity { get; set; }
}