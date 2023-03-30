// <copyright file="SendgridEvent.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Nest;
using OnlineSales.DataAnnotations;
using OnlineSales.Entities;

namespace OnlineSales.Plugin.SendGrid.Entities;

[Table("sendgrid_events")]
[SupportsChangeLog(SaveEntity = false)]
public class SendgridEvent : BaseEntityWithId
{
    public DateTime CreatedAt { get; set; }

    public string Event { get; set; } = string.Empty;

    public string MessageId { get; set; } = string.Empty;

    public string Reason { get; set; } = string.Empty;

    public string? Ip { get; set; } = string.Empty;

    public string? EventId { get; set; } = string.Empty;

    public int? ContactId { get; set; }

    [Ignore]
    [JsonIgnore]
    [ForeignKey("ContactId")]
    public virtual Contact? Contact { get; set; }
}