// <copyright file="OtpDetailsDto.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel.DataAnnotations;

namespace OnlineSales.Plugin.Sms.DTOs;

public class OtpDetailsDto
{
    [Required]
    public string Recipient { get; set; } = string.Empty;

    [Required]
    [MinLength(4)]
    [MaxLength(8)]
    [RegularExpression(@"^\d+$", ErrorMessage = "OTP code can contain digits only")]
    public string OtpCode { get; set; } = string.Empty;
}