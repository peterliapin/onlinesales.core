// <copyright file="EmailGroupDto.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel.DataAnnotations;
using OnlineSales.DataAnnotations;
using OnlineSales.Entities;

namespace OnlineSales.DTOs;

public class EmailGroupCreateDto
{
    [Required]
    public string Name { get; set; } = string.Empty;
}

public class EmailGroupUpdateDto
{
    [NonEmptyString]
    public string? Name { get; set; }
}