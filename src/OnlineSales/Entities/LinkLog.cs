// <copyright file="LinkLog.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using OnlineSales.DataAnnotations;

namespace OnlineSales.Entities;

[Table("link_log")]
public class LinkLog : BaseCreateByEntity
{
    [Required]
    public int LinkId { get; set; }

    [JsonIgnore]
    [ForeignKey("LinkId")]
    public virtual Link? Link { get; set; }

    [Required]
    [Searchable]
    public string Destination { get; set; } = string.Empty;

    public string? Referrer { get; set; }
}