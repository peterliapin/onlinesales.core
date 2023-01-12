// <copyright file="BaseFileSizeAttribute.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel.DataAnnotations;
using OnlineSales.Infrastructure;

namespace OnlineSales.DataAnnotations.Base
{
    public class BaseFileSizeAttribute : ValidationAttribute
    {
        public string MaxFileSize { get; set; } = string.Empty;

        protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
        {
            long? sizeInByte;

            var file = value as IFormFile;

            if (file == null)
            {
                return ValidationResult.Success!;
            }

            sizeInByte = StringHelper.GetSizeInBytesFromString(MaxFileSize);

            if (sizeInByte is null)
            {
                return new ValidationResult("Config Error, Measurement is invalid");
            }

            if (file.Length > sizeInByte)
            {
                return new ValidationResult("Max file size exceeded");
            }

            return ValidationResult.Success!;
        }
    }
}