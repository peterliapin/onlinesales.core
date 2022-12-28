// <copyright file="CustomerDtos.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel.DataAnnotations;

namespace OnlineSales.DTOs;

public class CustomerCreateDto
{
    public string LastName { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    public string CompanyName { get; set; } = string.Empty;

    public string Address1 { get; set; } = string.Empty;

    public string Address2 { get; set; } = string.Empty;

    public string State { get; set; } = string.Empty;

    public string Zip { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public int Timezone { get; set; } = 0;

    public string Language { get; set; } = string.Empty;
}

public class CustomerUpdateDto
{
    public string? LastName { get; set; }

    public string? FirstName { get; set; }

    [EmailAddress]
    public string? Email { get; set; }

    public string? CompanyName { get; set; }

    public string? Address1 { get; set; }

    public string? Address2 { get; set; }

    public string? State { get; set; }

    public string? Zip { get; set; }

    public string? Location { get; set; }

    public string? Phone { get; set; }

    public int? Timezone { get; set; }

    public string? Culture { get; set; }
}