// <copyright file="Account.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using OnlineSales.DataAnnotations;

namespace OnlineSales.Entities;

[Table("account")]
[SupportsChangeLog]
[Index(nameof(Name), IsUnique = true)]
public class Account : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public string? City { get; set; }

    public string? StateCode { get; set; }

    public string? Country { get; set; } 

    public string? EmployeesRate { get; set; } 

    public double? Revenue { get; set; }

    public int? DomainId { get; set; }

    [JsonIgnore]
    [DeleteBehavior(DeleteBehavior.Restrict)]
    [ForeignKey("DomainId")]
    public virtual Domain? Domain { get; set; }

    [Column(TypeName = "jsonb")]
    public string[]? Tags { get; set; }

    [Column(TypeName = "jsonb")]
    public Dictionary<string, string>? SocialMedia { get; set; }

    [Column(TypeName = "jsonb")]
    public string? Data { get; set; }
}
