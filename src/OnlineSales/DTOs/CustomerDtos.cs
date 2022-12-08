// <copyright file="CustomerDtos.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel.DataAnnotations;

namespace OnlineSales.DTOs;

public class CustomerCreateDto
{
    [Required]
    public string? LastName { get; set; }

    [Required]
    public string? FirstName { get; set; }

    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string? CompanyName { get; set; }

    public string? Address1 { get; set; }

    public string? Address2 { get; set; }

    public string? State { get; set; }

    public string? Zip { get; set; }

    public string? Location { get; set; }

    [Required]
    public string? Phone { get; set; }

    [Required]
    public int? Timezone { get; set; }

    public string? Culture { get; set; }
}

public class CustomerUpdateDto
{
    [Required]
    public string? LastName { get; set; }

    [Required]
    public string? FirstName { get; set; }

    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string? CompanyName { get; set; }

    public string? Address1 { get; set; }

    public string? Address2 { get; set; }

    public string? State { get; set; }

    public string? Zip { get; set; }

    public string? Location { get; set; }

    [Required]
    public string? Phone { get; set; }

    [Required]
    public int? Timezone { get; set; }

    public string? Culture { get; set; }
}