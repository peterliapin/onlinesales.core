// <copyright file="DealPipeline.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using OnlineSales.DataAnnotations;

namespace OnlineSales.Entities;

[Table("deal_pipeline")]
[SupportsElastic]
[SupportsChangeLog]
public class DealPipeline : BaseEntity
{
    [Required]
    [Searchable]
    public string Name { get; set; } = string.Empty;

    [JsonIgnore]
    public virtual ICollection<PipelineStage>? PipelineStages { get; set; }
}