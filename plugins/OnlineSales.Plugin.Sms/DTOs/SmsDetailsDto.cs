// <copyright file="SmsDetailsDto.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel.DataAnnotations;

namespace OnlineSales.Plugin.Sms.DTOs;

public class SmsDetailsDto
{
    [Required]
    public string Recipient { get; set; } = string.Empty;

    [Required]
    [MinLength(1)]
    public string Message { get; set; } = string.Empty;
}