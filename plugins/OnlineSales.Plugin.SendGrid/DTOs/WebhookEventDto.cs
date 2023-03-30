// <copyright file="WebhookEventDto.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

namespace OnlineSales.Plugin.SendGrid.DTOs;

public class WebhookEventDto : EmailDto
{
    public double Timestamp { get; set; } = 0;

    public string Event { get; set; } = string.Empty;

    public string Sg_Message_Id { get; set; } = string.Empty;

    public string Sg_Event_Id { get; set; } = string.Empty;

    public string Reason { get; set; } = string.Empty;

    public string? Ip { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;
}