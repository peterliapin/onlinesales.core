// <copyright file="Deal.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using OnlineSales.DataAnnotations;

namespace OnlineSales.Entities;

[Table("deal")]
[SupportsElastic]
[SupportsChangeLog]
public class Deal : BaseEntity
{
    public int? AccountId { get; set; }

    [JsonIgnore]
    [ForeignKey("AccountId")]
    public virtual Account? Account { get; set; }

    public int DealPipelineId { get; set; }

    [JsonIgnore]
    [ForeignKey("DealPipelineId")]
    public virtual DealPipeline? DealPipeline { get; set; }

    public int PipelineStageId { get; set; }

    [JsonIgnore]
    [ForeignKey("PipelineStageId")]
    public virtual DealPipelineStage? PipelineStage { get; set; }

    [JsonIgnore]
    public virtual ICollection<Contact>? Contacts { get; set; }

    public decimal DealValue { get; set; }

    /// <summary>
    /// Gets or sets the currency ISO code for the payment - ISO 4217. Example: "USD".
    /// </summary>
    [Searchable]
    [Required]
    public string DealCurrency { get; set; } = string.Empty;

    public DateTime? ExpectedCloseDate { get; set; }

    public DateTime? ActualCloseDate { get; set; }

    public string UserId { get; set; } = string.Empty; 

    [JsonIgnore]
    [ForeignKey("UserId")]
    public virtual User? CreatedBy { get; set; }
}