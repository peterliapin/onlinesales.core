// <copyright file="PipelineStage.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using OnlineSales.DataAnnotations;

namespace OnlineSales.Entities;

[Table("pipeline_stage")]
[SupportsElastic]
[SupportsChangeLog]
public class PipelineStage : BaseEntity
{
    [Required]
    [Searchable]
    public string Name { get; set; } = string.Empty;

    public int DealPipelineId { get; set; }

    [JsonIgnore]
    [ForeignKey("DealPipelineId")]
    public virtual DealPipeline? DealPipeline { get; set; }

    public int Order { get; set; }
}