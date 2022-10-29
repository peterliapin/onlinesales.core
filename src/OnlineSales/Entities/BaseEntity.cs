// <copyright file="BaseEntity.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace OnlineSales.Models;

public class BaseEntity
{
    public BaseEntity()
    {
    }

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; }

    [JsonIgnore]
    public string? CreatedByIP { get; set; } = string.Empty;

    [JsonIgnore]
    public string? CreatedByUserAgent { get; set; } = string.Empty;
   
    public DateTime? UpdatedAt { get; set; }

    [JsonIgnore]
    public string? UpdatedByIP { get; set; } = string.Empty;

    [JsonIgnore]
    public string? UpdatedByUserAgent { get; set; } = string.Empty;
}