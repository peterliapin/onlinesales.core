// <copyright file="ActivityLogDto.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel.DataAnnotations;
using CsvHelper.Configuration.Attributes;
using OnlineSales.DataAnnotations;
using OnlineSales.Entities;

namespace OnlineSales.DTOs;

public class ActivityLogDto
{
    public int Id { get; set; }

    public string Source { get; set; } = string.Empty;

    public int SourceId { get; set; }

    public string Type { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public int? ContactId { get; set; }

    public string? Ip { get; set; }

    public string Data { get; set; } = string.Empty;
}