// <copyright file="ImapAccountDtos.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel.DataAnnotations;

namespace OnlineSales.Plugin.EmailSync.DTOs;

public class ImapAccountBaseDto
{
    [Required]
    public string Host { get; set; } = string.Empty;

    [Required]
    public string UserName { get; set; } = string.Empty;

    [Required]
    public int Port { get; set; }

    [Required]
    public bool UseSsl { get; set; }
}

public class ImapAccountCreateDto : ImapAccountBaseDto
{
    [Required]
    public string Password { get; set; } = string.Empty;
}

public class ImapAccountUpdateDto
{
    public string? Host { get; set; } = string.Empty;

    public string? UserName { get; set; } = string.Empty;

    public string? Password { get; set; } = string.Empty;

    public int? Port { get; set; }

    public bool? UseSsl { get; set; }
}

public class ImapAccountDetailsDto : ImapAccountBaseDto
{
    public int Id { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
