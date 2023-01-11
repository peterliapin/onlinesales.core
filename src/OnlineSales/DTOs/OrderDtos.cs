// <copyright file="OrderDtos.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel.DataAnnotations;
using CsvHelper.Configuration.Attributes;

namespace OnlineSales.DTOs;

public class OrderCreateDto
{
    [Required]
    public int CustomerId { get; set; }

    [Required]
    public string RefNo { get; set; } = string.Empty;

    public string? OrderNumber { get; set; } = string.Empty;

    public string? AffiliateName { get; set; }

    [Required]
    public decimal ExchangeRate { get; set; } = 1;

    [Required]
    public string Currency { get; set; } = string.Empty;

    public bool TestOrder { get; set; } = false;

    public string? Data { get; set; }
}

public class OrderUpdateDto
{
    [Required]
    public string RefNo { get; set; } = string.Empty;

    public string? AffiliateName { get; set; }

    public string? Data { get; set; }
}

public class OrderDetailsDto : OrderCreateDto
{
    public int Id { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int Quantity { get; set; }

    public decimal Total { get; set; }
}

public class OrderImportDto : OrderCreateDto
{
    [Optional]
    public int? Id { get; set; }

    [Optional]
    public DateTime? CreatedAt { get; set; }

    [Optional]
    public DateTime? UpdatedAt { get; set; }

    [Optional]
    public string? CreatedByIp { get; set; }

    [Optional]
    public string? CreatedByUserAgent { get; set; }

    [Optional]
    public string? UpdatedByIp { get; set; }

    [Optional]
    public string? UpdatedByUserAgent { get; set; }
}