// <copyright file="BaseImportDtos.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using CsvHelper.Configuration.Attributes;

namespace OnlineSales.DTOs;

public class BaseImportDtoWithIdAndSource
{
    [Optional]
    public int? Id { get; set; }

    [Optional]
    public string? Source { get; set; }
}

public class BaseImportDtoWithDates : BaseImportDtoWithIdAndSource
{
    [Optional]
    public DateTime? CreatedAt { get; set; }

    [Optional]
    public DateTime? UpdatedAt { get; set; }
}

public class BaseImportDto : BaseImportDtoWithDates
{
    [Optional]
    public string? CreatedByIp { get; set; }

    [Optional]
    public string? CreatedByUserAgent { get; set; }

    [Optional]
    public string? UpdatedByIp { get; set; }

    [Optional]
    public string? UpdatedByUserAgent { get; set; }
}