// <copyright file="ImageFileSizeAttribute.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;
using OnlineSales.Configuration;
using OnlineSales.DataAnnotations.Base;

namespace OnlineSales.DataAnnotations
{
    public class ImageFileSizeAttribute : BaseFileSizeAttribute
    {
        protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
        {
            var configuration = (IOptions<ImagesConfig>)validationContext!.GetService(typeof(IOptions<ImagesConfig>)) !;
            string maxFileSize = configuration.Value.MaxSize!;

            this.MaxFileSize = maxFileSize;
            return base.IsValid(value, validationContext);
        }
    }
}