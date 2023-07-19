// <copyright file="OrderItem.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Nest;
using OnlineSales.DataAnnotations;

namespace OnlineSales.Entities;

[SupportsElastic]
[SupportsChangeLog]
[Table("order_item")]
public class OrderItem : BaseEntity
{
    /// <summary>
    /// Gets or sets reference to Order table.
    /// </summary>
    [Required]
    public int OrderId { get; set; }

    /// <summary>
    /// Gets or sets a unique reference numbers for order item.
    /// </summary>
    [Searchable]
    public string? RefNo { get; set; } = string.Empty;

    [JsonIgnore]
    [ForeignKey("OrderId")]
    public virtual Order? Order { get; set; }

    /// <summary>
    /// Gets or sets the name of the product as defined by vendor.
    /// </summary>
    [Searchable]
    [Required]
    public string ProductName { get; set; } = string.Empty;

    /// <summary>
    /// Gets total amount converted to a system currency (or payout currency) like USD without TAXes, discounts and commissions (how much will be paid out to vendor).
    /// </summary>
    [Searchable]
    [Required]
    public decimal Total { get; internal set; } = 0;

    /// <summary>
    /// Gets or sets the currency ISO code for the payment - ISO 4217. Example: "USD".
    /// </summary>
    [Searchable]
    [Required]
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    /// Gets total amount in the payment currency without TAXes, discounts and commissions.
    /// </summary>
    [Searchable]
    [Required]
    public decimal CurrencyTotal { get; internal set; } = 0;

    /// <summary>
    /// Gets or sets total amount of all items in the current order.
    /// </summary>
    [Searchable]
    public int Quantity { get; set; }

    /// <summary>
    /// Gets or sets unit price in the payment currency without TAXes, discounts and commissions.
    /// </summary>
    [Searchable]
    public decimal UnitPrice { get; set; }

    public int? DiscountId { get; set; }

    [JsonIgnore]
    [ForeignKey("DiscountId")]
    public virtual Discount? Discount { get; set; }
}