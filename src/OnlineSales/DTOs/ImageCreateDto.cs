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
        [FileSizeValidateAttribute(10485760, ErrorMessage = "Max file size exceeded")]
        [FileExtensionValidateAttribute(".jpg;.png;.gif", ErrorMessage = "Extension is not valid")]
        public IFormFile? Image { get; set; }

        [Required]
        public string ScopeId { get; set; } = string.Empty;
    }
}
