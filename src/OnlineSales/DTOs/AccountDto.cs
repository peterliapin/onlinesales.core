// <copyright file="AccountDto.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel.DataAnnotations;
using CsvHelper.Configuration.Attributes;
using OnlineSales.Geography;

namespace OnlineSales.DTOs;

public class AccountCreateDto
{
    [Required]
    public string Name { get; set; } = string.Empty;

    public string? State { get; set; }

    [Optional]
    public string? ContinentCode { get; set; }

    [Optional]
    public string? CountryCode { get; set; }

    [Optional]
    public string? CityName { get; set; }

    [Optional]
    public string? SiteUrl { get; set; }

    public string? EmployeesRange { get; set; }

    public double? Revenue { get; set; }

    public string[]? Tags { get; set; }

    public Dictionary<string, string>? SocialMedia { get; set; }

    public string? Source { get; set; }

    [Optional]
    public string? Data { get; set; }
}

public class AccountDetailsInfo
{
    public string? Name { get; set; }

    public string? CityName { get; set; }

    public string? State { get; set; }

    public string? CountryCode { get; set; }

    public string? EmployeesRange { get; set; }

    public double? Revenue { get; set; }

    public string[]? Tags { get; set; }

    public Dictionary<string, string>? SocialMedia { get; set; }

    public string? Source { get; set; }

    public string? Data { get; set; }
}

public class AccountUpdateDto
{
    public string? Name { get; set; }

    public string? City { get; set; }

    public string? StateCode { get; set; }

    public string? CountryCode { get; set; }

    public string? EmployeesRange { get; set; }

    public double? Revenue { get; set; }

    public string[]? Tags { get; set; }

    public Dictionary<string, string>? SocialMedia { get; set; }

    public string? Source { get; set; }

    public string? Data { get; set; }
}

public class AccountDetailsDto : AccountCreateDto
{
    public int Id { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}

public class AccountImportDto : BaseImportDto
{
    [Optional]
    public string Name { get; set; } = string.Empty;

    [Optional]
    public string? City { get; set; }

    [Optional]
    public string? StateCode { get; set; }

    [Optional]
    public string? CountryCode { get; set; }

    [Optional]
    public string? SiteUrl { get; set; }

    [Optional]
    public string? EmployeesRange { get; set; }

    [Optional]
    public double? Revenue { get; set; }

    [Optional]
    public string[]? Tags { get; set; }

    [Optional]
    public Dictionary<string, string>? SocialMedia { get; set; }

    [Optional]
    public string? Data { get; set; }
}
