// <copyright file="OrderItemDtos.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel.DataAnnotations;
using CsvHelper.Configuration.Attributes;
using OnlineSales.DataAnnotations;
using OnlineSales.Entities;

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

    [Optional]
    public string? Source { get; set; }
}

public class OrderItemUpdateDto
{
    [MinLength(1)]
    public string? ProductName { get; set; }

    [MinLength(1)]
    public string? LicenseCode { get; set; }

    public decimal? UnitPrice { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Minimum quantity should be 1")]
    public int? Quantity { get; set; }

    public string? Data { get; set; }
}

public class OrderItemDetailsDto : OrderItemCreateDto
{
    public int Id { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public decimal CurrencyTotal { get; set; } = 0;

    public decimal Total { get; set; } = 0;

    [Ignore]
    public OrderDetailsDto? Order { get; set; }
}

public class OrderItemImportDto : BaseImportDto
{
    [Optional]
    public int? OrderId { get; set; }

    [Optional]
    [SurrogateForeignKey(typeof(Order), "RefNo", "OrderId")]
    public string? OrderRefNo { get; set; } = string.Empty;

    [Optional]
    public string? ProductName { get; set; } = string.Empty;

    [Optional]
    public string? LicenseCode { get; set; } = string.Empty;

    [Optional]
    public decimal? UnitPrice { get; set; } = 0;

    [Optional]
    [CurrencyCode]
    public string? Currency { get; set; }

    [Optional]
    public int? Quantity { get; set; }
}