// <copyright file="Domain.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace OnlineSales.Entities;

[Table("domain")]
[Index(nameof(Name), IsUnique = true)]
public class Domain : BaseEntityWithIdAndDates
{
    public bool Shared { get; set; }

    public bool Disposable { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;
}

