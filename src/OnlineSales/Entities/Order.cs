// <copyright file="Order.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace OnlineSales.Entities;

    public class Order : BaseEntity
    {
        /// <summary>
        /// Gets or sets reference to a customer table.
        /// </summary>
        [Required]
        public int CustomerId { get; set; }

        [JsonIgnore]
        [ForeignKey("CustomerId")]
        public virtual Customer? Customer { get; set; }

        /// <summary>
        /// Gets or sets customer IP address detected by the payment processing system.
        /// </summary>
        public string? CustomerIP { get; set; }

        /// <summary>
        /// Gets or sets a unique reference numbers across all orders.
        /// </summary>
        [Required]
        public string RefNo { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets internal consecutive order number associates with orders provided by payment processing system.
        /// </summary>
        public string? OrderNumber { get; set; }

        /// <summary>
        /// Gets or sets total amount converted to a system currency (or payout currency) like USD without TAXes, discounts and comissions (how much will be paid out to vendor).
        /// </summary>
        [Required]
        public decimal Total { get; set; } = 0;

        /// <summary>
        /// Gets or sets the currency ISO code for the payment - ISO 4217. Example: "USD".
        /// </summary>
        [Required]
        public string Currency { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets total amount in the payment currency without TAXes, discounts and comissions (how much will be paid out to vendor).
        /// </summary>
        [Required]
        public decimal CurrencyTotal { get; set; } = 0;

        /// <summary>
        /// Gets or sets total amount of all items in the current order.
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Gets or sets affiliate Name.
        /// </summary>
        public string? AffiliateName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether true for test orders. False of regular orders.
        /// </summary>
        public bool TestOrder { get; set; } = false;
    }

