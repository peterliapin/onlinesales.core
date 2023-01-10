// <copyright file="OrderItemDtos.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel.DataAnnotations;
using CsvHelper.Configuration.Attributes;
using Microsoft.AspNetCore.Http.HttpResults;
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
    [NonEmptyString]
    public string? ProductName { get; set; }

    [NonEmptyString]
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
}

public class OrderItemImportDto : OrderItemCreateDto
{
    private DateTime? createdAt;

    private DateTime? updatedAt;

    [Optional]
    public int? Id { get; set; }

    [Required]
    public string OrderRefNo { get; set; } = string.Empty;

    [Optional]
    public DateTime? CreatedAt
    {
        get { return createdAt; }
        set { createdAt = value is not null ? DateTime.SpecifyKind((DateTime)value, DateTimeKind.Utc) : value; }
    }

    [Optional]
    public DateTime? UpdatedAt
    {
        get { return updatedAt; }
        set { updatedAt = value is not null ? DateTime.SpecifyKind((DateTime)value, DateTimeKind.Utc) : value; }
    }

    [Optional]
    public string? CreatedByIp { get; set; }

    [Optional]
    public string? CreatedByUserAgent { get; set; }

    [Optional]
    public string? UpdatedByIp { get; set; }

    [Optional]
    public string? UpdatedByUserAgent { get; set; }
}