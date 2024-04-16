// <copyright file="Promotion.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using OnlineSales.DataAnnotations;

namespace OnlineSales.Entities;

[Table("promotion")]
[SupportsElastic]
[SupportsChangeLog]
[Index(nameof(Code), IsUnique = true)]

public class Promotion : BaseEntity 
{
    [Required]
    public string Code { get; set; } = string.Empty;

    [Required]
    public string Name { get; set; } = string.Empty;

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }
}