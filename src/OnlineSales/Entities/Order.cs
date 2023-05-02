// <copyright file="Order.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Nest;
using OnlineSales.DataAnnotations;

namespace OnlineSales.Entities;

[Table("order")]
[SupportsElastic]
[SupportsChangeLog]
[Index(nameof(RefNo), IsUnique = true)]
public class Order : BaseEntity
{
    /// <summary>
    /// Gets or sets reference to a contact table.
    /// </summary>
    [Required]
    public int ContactId { get; set; }

    [ForeignKey("ContactId")]
    public virtual Contact? Contact { get; set; }

    /// <summary>
    /// Gets or sets contact  address detected by the payment processing system.
    /// </summary>
    public string? ContactIp { get; set; }

    /// <summary>
    /// Gets or sets a unique reference numbers across all orders.
    /// </summary>
    [Searchable]
    [Required]
    public string RefNo { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets internal consecutive order number associates with orders provided by payment processing system.
    /// </summary>
    public string? OrderNumber { get; set; }

    /// <summary>
    /// Gets or sets total amount converted to a system currency (or payout currency) like USD without TAXes, discounts and commissions (how much will be paid out to vendor).
    /// </summary>
    [Searchable]
    [Required]
    public decimal Total { get; set; } = 0;

    /// <summary>
    /// Gets or sets the currency ISO code for the payment - ISO 4217. Example: "USD".
    /// </summary>
    [Searchable]
    [Required]
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets total amount in the payment currency without TAXes, discounts and comissions.
    /// </summary>
    [Searchable]
    [Required]
    public decimal CurrencyTotal { get; set; } = 0;

    /// <summary>
    /// Gets or sets total amount of all items in the current order.
    /// </summary>
    [Searchable]
    public int Quantity { get; set; }

    /// <summary>
    /// Gets or sets exchange rate to the payout currency.
    /// </summary>
    [Searchable]
    public decimal ExchangeRate { get; set; }

    /// <summary>
    /// Gets or sets affiliate Name.
    /// </summary>
    [Searchable]
    public string? AffiliateName { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether true for test orders. False of regular orders.
    /// </summary>
    public bool TestOrder { get; set; } = false;

    [Column(TypeName = "jsonb")]
    public string? Data { get; set; }

    [Nest.Ignore]
    public virtual ICollection<OrderItem>? OrderItems { get; set; }
}