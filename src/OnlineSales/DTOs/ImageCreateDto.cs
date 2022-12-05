// <copyright file="ImageCreateDto.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel.DataAnnotations;
using OnlineSales.DataAnnotations;

namespace OnlineSales.DTOs
{
    public class ImageCreateDto
    {
        [Required]
        [ImageExtension]
        [ImageFileSize]
        public IFormFile? Image { get; set; }

        [Required]
        public string ScopeUid { get; set; } = string.Empty;
    }
}
