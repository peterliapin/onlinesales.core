// <copyright file="BaseEntity.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineSales.Entities;

public class BaseEntity : BaseCreateByEntity
{
    public DateTime? UpdatedAt { get; set; }

    public string? UpdatedByIp { get; set; }

    public string? UpdatedByUserAgent { get; set; }
}

public class BaseCreateByEntity : BaseEntityWithId
{
    [Required]
    public DateTime CreatedAt { get; set; }

    public string? CreatedByIp { get; set; }

    public string? CreatedByUserAgent { get; set; }
}

public class BaseEntityWithId
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
}