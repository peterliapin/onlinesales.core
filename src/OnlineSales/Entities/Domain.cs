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
    [Required]
    public string Name { get; set; } = string.Empty;

    public string? Title { get; set; }

    public string? Description { get; set; }

    public string? Url { get; set; }

    public bool? HttpCheck { get; set; }

    public bool? Free { get; set; }

    public bool? Disposable { get; set; }

    public bool? CatchAll { get; set; }

    [Column(TypeName = "jsonb")]
    public string? DnsRecords { get; set; }

    public bool? DnsCheck { get; set; }
}


