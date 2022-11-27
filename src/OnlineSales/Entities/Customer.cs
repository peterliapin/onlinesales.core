// <copyright file="Customer.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineSales.Entities;

[Table("customer")]
public class Customer : BaseEntity
{
    public string? LastName { get; set; }

    public string? FirstName { get; set; }

    [Required]
    public string Email { get; set; } = string.Empty;

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
