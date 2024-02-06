// <copyright file="IdentityDtos.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel.DataAnnotations;

namespace OnlineSales.DTOs;

public class LoginDto
{
    [Required]
    [EmailAddress]
    required public string Email { get; set; }

    [Required]
    required public string Password { get; set; }
}

public class JWTokenDto
{
    [Required]
    required public string AccessToken { get; set; }

    [Required]
    required public string RefreshToken { get; set; }

    [Required]
    required public DateTime Expiration { get; set; }

    [Required]
    required public string TokenType { get; set; }
}

public class RefreshTokenDto
{
    [Required]
    required public string RefreshToken { get; set; }
}