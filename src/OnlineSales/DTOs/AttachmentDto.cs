// <copyright file="AttachmentDto.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel.DataAnnotations;

namespace OnlineSales.DTOs;

public class AttachmentDto
{
    /// <summary>
    /// Gets or sets the email attachment converted to a byte array.
    /// </summary>
    [Required]
    public byte[] File { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// Gets or sets the attachment file name with the extension.
    /// </summary>
    [Required]
    public string FileName { get; set; } = string.Empty;
}