// <copyright file="ContactDtos.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel.DataAnnotations;
using CsvHelper.Configuration.Attributes;
using OnlineSales.DataAnnotations;
using OnlineSales.Entities;
using OnlineSales.Geography;

namespace OnlineSales.DTOs;

public abstract class BaseContactDto
{
    public string? LastName { get; set; }

    public string? FirstName { get; set; }

    public Continent? ContinentCode { get; set; }

    public Country? CountryCode { get; set; }

    public string? CityName { get; set; }

    public string? Address1 { get; set; }

    public string? Address2 { get; set; }

    public string? State { get; set; }

    public string? Zip { get; set; }

    public string? Phone { get; set; }

    public int? Timezone { get; set; }

    public string? Language { get; set; }

    [SwaggerHide]
    public int? UnsubscribeId { get; set; }

    public string? Source { get; set; }
}

public class ContactCreateDto : BaseContactDto
{
    private string email = string.Empty;

    [Required]
    [EmailAddress]
    public string Email
    {
        get
        {
            return email;
        }

        set
        {
            email = value.ToLower();
        }
    }
}

public class ContactUpdateDto : BaseContactDto
{
    private string? email;

    [EmailAddress]
    public string? Email
    {
        get
        {
            return email;
        }

        set
        {
            email = value == null ? null : value.ToLower();
        }
    }
}

public class ContactDetailsDto : ContactCreateDto
{
    public int Id { get; set; }

    public string AvatarUrl { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int DomainId { get; set; }
}

public class ContactImportDto : BaseImportDto
{
    private string? email;

    [Optional]
    [EmailAddress]
    [SwaggerUnique]
    public string? Email
    {
        get
        {
            return email;
        }

        set
        {
            email = value == null ? null : value.ToLower();
        }
    }

    [Optional]
    public string? LastName { get; set; }

    [Optional]
    public string? FirstName { get; set; }

    [Optional]
    public Continent? ContinentCode { get; set; }

    [Optional]
    public Country? CountryCode { get; set; }

    [Optional]
    public string? CityName { get; set; }

    [Optional]
    public string? Address1 { get; set; }

    [Optional]
    public string? Address2 { get; set; }

    [Optional]
    public string? State { get; set; }

    [Optional]
    public string? Zip { get; set; }

    [Optional]
    public string? Phone { get; set; }

    [Optional]
    public int? Timezone { get; set; }

    [Optional]
    public string? Language { get; set; }

    [Optional]
    public int? UnsubscribeId { get; set; }

    [Optional]
    public int? AccountId { get; set; }

    [Optional]
    [SurrogateForeignKey(typeof(Account), "Name", "AccountId")]
    public string? AccountName { get; set; }

    [Optional]
    public int? DomainId { get; set; }
}