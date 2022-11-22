// <copyright file="GetshoutoutMessageDto.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

namespace OnlineSales.Plugin.Sms.DTOs;

public class GetshoutoutMessageDto
{
    public string Source { get; set; } = string.Empty;

    public List<string> Destinations { get; set; } = new List<string>();

    public List<string> Transports { get; set; } = new List<string> { "sms" };

    public Content Content { get; set; } = new Content();
}

public class Content
{
    public string Sms { get; set; } = string.Empty;
}
