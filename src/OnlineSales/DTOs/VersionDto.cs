// <copyright file="VersionDto.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using Microsoft.Extensions.Primitives;

namespace OnlineSales.DTOs;

public class VersionDto
{
    public string? Version { get; set; } = string.Empty;

    public string? IP { get; set; } = string.Empty;

    public string? IPv4 { get; set; } = string.Empty;

    public string? IPv6 { get; set; } = string.Empty;

    public List<KeyValuePair<string, StringValues>> Headers { get; set; } = new List<KeyValuePair<string, StringValues>>();
}