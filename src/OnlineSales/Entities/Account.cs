// <copyright file="Account.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using OnlineSales.DataAnnotations;
using OnlineSales.Geography;

namespace OnlineSales.Entities;

[Table("account")]
[SupportsElastic]
[SupportsChangeLog]
[Index(nameof(Name), IsUnique = true)]
public class Account : BaseEntity
{
    [Searchable]
    public string Name { get; set; } = string.Empty;

    [Searchable]
    public string? CityName { get; set; }

    [Searchable]
    public string? State { get; set; }

    [Searchable]
    public Continent? ContinentCode { get; set; }

    [Searchable]
    public Country? CountryCode { get; set; }

    [Searchable]
    public string? SiteUrl { get; set; }

    public string? LogoUrl { get; set; }

    [Searchable]
    public string? EmployeesRange { get; set; }

    [Searchable]
    public double? Revenue { get; set; }

    [Searchable]
    [Column(TypeName = "jsonb")]
    public string[]? Tags { get; set; }

    [Searchable]
    [Column(TypeName = "jsonb")]
    public Dictionary<string, string>? SocialMedia { get; set; }

    [Searchable]
    [Column(TypeName = "jsonb")]
    public string? Data { get; set; }
}