// <copyright file="IdentityDtos.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel.DataAnnotations;
using CsvHelper.Configuration.Attributes;

namespace OnlineSales.DTOs;

public class LoginDto
{
    [Required]
    required public string Email { get; set; }

    [Optional]
    public string Password { get; set; } = string.Empty;
}
