// <copyright file="Unsubscribe.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Nest;
using OnlineSales.DataAnnotations;

namespace OnlineSales.Entities;

[Table("unsubscribe")]
[SupportsChangeLog]
[SupportsElastic]
public class Unsubscribe : BaseCreateByEntity
{
    public string Reason { get; set; } = string.Empty;
    
    public int? ContactId { get; set; }

    [Nest.Ignore]
    [JsonIgnore]
    [ForeignKey("ContactId")]
    public virtual Contact? Contact { get; set; }
}