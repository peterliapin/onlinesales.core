// <copyright file="Contact.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Nest;
using OnlineSales.DataAnnotations;
using OnlineSales.Geography;

namespace OnlineSales.Entities;

[Table("contact")]
[SupportsElastic]
[SupportsChangeLog]
[Index(nameof(Email), IsUnique = true)]
public class Contact : BaseEntity
{
    private string email = string.Empty;

    [Searchable]
    public string? LastName { get; set; }

    [Searchable]
    public string? FirstName { get; set; }

    [Required]
    [Searchable]
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

    [Searchable]
    public Continent? ContinentCode { get; set; }

    [Searchable]
    public Country? CountryCode { get; set; }

    [Searchable]
    public string? CityName { get; set; }

    [Searchable]
    public string? Address1 { get; set; }

    [Searchable]
    public string? Address2 { get; set; }

    [Searchable]
    public string? State { get; set; }

    [Searchable]
    public string? Zip { get; set; }

    [Searchable]
    public string? Phone { get; set; }

    public int? Timezone { get; set; }

    [Searchable]
    public string? Language { get; set; }

    [Required]
    public int DomainId { get; set; }

    [JsonIgnore]
    [ForeignKey("DomainId")]
    [DeleteBehavior(DeleteBehavior.Restrict)]
    public virtual Domain? Domain { get; set; }

    public int? AccountId { get; set; }

    [JsonIgnore]
    [ForeignKey("AccountId")]
    [DeleteBehavior(DeleteBehavior.Restrict)]
    public virtual Account? Account { get; set; }
    
    public int? UnsubscribeId { get; set; }

    [JsonIgnore]
    [ForeignKey("UnsubscribeId")]
    [DeleteBehavior(DeleteBehavior.SetNull)]
    public virtual Unsubscribe? Unsubscribe { get; set; }

    [JsonIgnore]
    public virtual ICollection<Order>? Orders { get; set; }
}