// <copyright file="Link.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using OnlineSales.DataAnnotations;

namespace OnlineSales.Entities;

[Table("link")]
[Index(nameof(Uid), IsUnique = true)]
[SupportsChangeLog]
public class Link : BaseEntity
{
    [Required]
    public string Uid { get; set; } = string.Empty;

    [Required]
    public string Destination { get; set; } = string.Empty;

    [Required]
    public string Name { get; set; } = string.Empty;
}