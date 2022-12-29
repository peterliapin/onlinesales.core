// <copyright file="BaseEntity.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace OnlineSales.Entities;

public class BaseEntity : BaseEntityWithId
{
    [Required]
    public DateTime CreatedAt { get; set; }

    public string? CreatedByIp { get; set; } = string.Empty;

    [JsonIgnore]
    public string? CreatedByUserAgent { get; set; } = string.Empty;

    public DateTime? UpdatedAt { get; set; }

    public string? UpdatedByIp { get; set; } = string.Empty;

    [JsonIgnore]
    public string? UpdatedByUserAgent { get; set; } = string.Empty;
}

public class BaseEntityWithId
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
}