// <copyright file="UnsubscribeDtos.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

namespace OnlineSales.DTOs;

public class UnsubscribeDetailsDto
{
    public string Source { get; set; } = string.Empty;

    public string Reason { get; set; } = string.Empty;

    public int? ContactId { get; set; }
}