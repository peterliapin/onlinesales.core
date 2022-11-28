// <copyright file="ImageCreateDto.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel.DataAnnotations;
using OnlineSales.CustomAttributeValidations;
using OnlineSales.CustomValidations;

namespace OnlineSales.DTOs
{
    public class ImageCreateDto
    {
        [Required]
        [FileSizeValidate("Image")]
        [FileExtensionValidate("Image")]
        public IFormFile? Image { get; set; }

        [Required]
        public string ScopeId { get; set; } = string.Empty;
    }
}
