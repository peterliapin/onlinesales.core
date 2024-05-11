// <copyright file="ImportEventDto.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using CsvHelper.Configuration.Attributes;

namespace OnlineSales.Plugin.SendGrid.DTOs;

public class ImportEventDto : EmailDto
{
    public string Processed { get; set; } = string.Empty;

    [Name("message_id")]
    public string MessageId { get; set; } = string.Empty;

    public string Event { get; set; } = string.Empty;

    public string? Type { get; set; } = string.Empty;

    public string? Reason { get; set; } = string.Empty;
}