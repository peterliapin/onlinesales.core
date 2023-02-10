// <copyright file="Contact.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Nest;
using OnlineSales.DataAnnotations;

namespace OnlineSales.Entities;

[Table("contact")]
[SupportsElastic]
[SupportsChangeLog]
[Index(nameof(Email), IsUnique = true)]
public class Contact : BaseEntity
{
    [Searchable]
    public string? LastName { get; set; }

    [Searchable]
    public string? FirstName { get; set; }

    [Searchable]
    [Required]
    public string Email { get; set; } = string.Empty;

    public string? CompanyName { get; set; }

    [Searchable]
    public string? Address1 { get; set; }

    [Searchable]
    public string? Address2 { get; set; }

    public string? State { get; set; }

    public string? Zip { get; set; }

    public string? Location { get; set; }

    [Searchable]
    public string? Phone { get; set; }

    public int? Timezone { get; set; }

    [Searchable]
    public string? Language { get; set; }

    [Required]
    public int DomainId { get; set; }

    [Ignore]
    [JsonIgnore]
    [ForeignKey("DomainId")]
    [DeleteBehavior(DeleteBehavior.Restrict)]
    public virtual Domain? Domain { get; set; }

    public int? AccountId { get; set; }

    [DeleteBehavior(DeleteBehavior.Restrict)]
    [JsonIgnore]
    [ForeignKey("AccountId")]
    public virtual Account? Account { get; set; }

    public string? Source { get; set; }
}