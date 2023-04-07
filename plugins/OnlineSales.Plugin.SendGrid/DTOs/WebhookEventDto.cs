// <copyright file="WebhookEventDto.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using Newtonsoft.Json;

namespace OnlineSales.Plugin.SendGrid.DTOs;

public class WebhookEventDto : EmailDto
{
    public double Timestamp { get; set; } = 0;

    public string Event { get; set; } = string.Empty;

    [JsonProperty("sg_message_id")]
    public string SendGridMessageId { get; set; } = string.Empty;

    [JsonProperty("sg_event_id")]
    public string SendGridEventId { get; set; } = string.Empty;

    public string Reason { get; set; } = string.Empty;

    public string? Ip { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;
}