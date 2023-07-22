// <copyright file="Discount.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using OnlineSales.DataAnnotations;

namespace OnlineSales.Entities;

[Table("discount")]
[SupportsChangeLog]

public class Discount : BaseEntity
{
    [Required]
    public int PromotionId { get; set; }

    [JsonIgnore]
    [ForeignKey("PromotionId")]
    public virtual Promotion? Promotion { get; set; }

    public int? OrderItemId { get; set; }

    [JsonIgnore]
    [ForeignKey("OrderItemId")]
    public virtual OrderItem? OrderItem { get; set; }

    public int? OrderId { get; set; }

    [JsonIgnore]
    [ForeignKey("OrderId")]
    public virtual Order? Order { get; set; }

    [Required]
    public decimal Value { get; set; }
}