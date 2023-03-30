// <copyright file="MessageEventDto.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

namespace OnlineSales.Plugin.SendGrid.DTOs;

public class MessageEventDto : EmailDto
{
    public string Processed { get; set; } = string.Empty;

    public string Message_id { get; set; } = string.Empty;

    public string Event { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;

    public string Reason { get; set; } = string.Empty;

    public string? Originating_ip { get; set; } = string.Empty;
}