// <copyright file="ContactDtos.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel.DataAnnotations;
using CsvHelper.Configuration.Attributes;
using OnlineSales.DataAnnotations;
using OnlineSales.Entities;

namespace OnlineSales.DTOs;

public abstract class BaseContactDto
{
    [Optional]
    public string? LastName { get; set; }

    [Optional]
    public string? FirstName { get; set; }

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
}

public class ContactCreateDto : BaseContactDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}

public class ContactUpdateDto : BaseContactDto
{
    [EmailAddress]
    public string? Email { get; set; }
}

public class ContactDetailsDto : ContactCreateDto
{
    public int Id { get; set; }

    public string AvatarUrl { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int DomainId { get; set; }
}

public class ContactImportDto : ContactCreateDto
{
    [Optional]
    public int? Id { get; set; }

    [Optional]
    public DateTime? CreatedAt { get; set; }

    [Optional]
    public DateTime? UpdatedAt { get; set; }

    [Optional]
    public string? CreatedByIp { get; set; }

    [Optional]
    public string? CreatedByUserAgent { get; set; }

    [Optional]
    public string? UpdatedByIp { get; set; }

    [Optional]
    public string? UpdatedByUserAgent { get; set; }

    [Optional]
    public string? Source { get; set; }

    [Optional]
    public int? AccountId { get; set; }

    [Optional]
    public int DomainId { get; set; }

    [Optional]
    [SurrogateForeignKey(typeof(Account), "Name", "AccountId")]
    public string? AccountName { get; set; }
}