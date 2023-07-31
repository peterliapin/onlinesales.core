// <copyright file="ContactIP.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Newtonsoft.Json;

using OnlineSales.DataAnnotations;

namespace OnlineSales.Entities;

[Table("contact_ip")]
[SupportsElastic]
[SupportsChangeLog]
public class ContactIP : BaseEntity
{
    /// <summary>
    /// Gets or sets reference to the contact table.
    /// </summary>
    [Required]
    [Searchable]
    public int ContactId { get; set; }

    [JsonIgnore]
    [ForeignKey("ContactId")]
    public virtual Contact? Contact { get; set; }

    [Searchable]
    public string IpAddress { get; set; } = string.Empty;
}