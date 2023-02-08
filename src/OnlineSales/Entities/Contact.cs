// <copyright file="Contact.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using OnlineSales.DataAnnotations;

namespace OnlineSales.Entities;

[Table("contact")]
[Index(nameof(Email), IsUnique = true)]
[SupportsElasticSearch]
[SupportsChangeLog]
public class Contact : BaseEntity
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

    public string? Language { get; set; }

    [Required]
    public int DomainId { get; set; }

    [DeleteBehavior(DeleteBehavior.Restrict)]
    [JsonIgnore]
    [ForeignKey("DomainId")]
    public virtual Domain? Domain { get; set; }

    public int? AccountId { get; set; }

    [DeleteBehavior(DeleteBehavior.Restrict)]
    [JsonIgnore]
    [ForeignKey("AccountId")]
    public virtual Account? Account { get; set; }

    public string? Source { get; set; }
}