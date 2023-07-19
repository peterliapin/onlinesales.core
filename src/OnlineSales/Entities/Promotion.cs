﻿// <copyright file="Promotion.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Nest;
using OnlineSales.DataAnnotations;

namespace OnlineSales.Entities;
public enum PromotionType
{
    Percent = 0,
    Fixed = 1,
}

[Table("promotion")]
[SupportsElastic]
[SupportsChangeLog]
[Index(nameof(PromotionCode), IsUnique = true)]

public class Promotion : BaseEntity
{
    [Required]
    public string PromotionCode { get; set; } = string.Empty;

    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }
}