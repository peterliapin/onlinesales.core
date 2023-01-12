// <copyright file="FileSizeValidateAttribute.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;
using OnlineSales.Configuration;
using OnlineSales.Infrastructure;

namespace OnlineSales.DataAnnotations
{
    public class FileSizeValidateAttribute : ValidationAttribute
    {
        private readonly string type;

        public FileSizeValidateAttribute(string type)
        {
            this.type = type;
        }

        protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
        {
            string maxFileSize;
            long? sizeInByte;

            if (type.Equals("Image"))
            {
                var configuration = (IOptions<ImagesConfig>)validationContext!.GetService(typeof(IOptions<ImagesConfig>)) !;
                maxFileSize = configuration.Value.MaxSize!;
            }
            else
            {
                return new ValidationResult("Invalid request type (check annotation)");
            }

            if (string.IsNullOrEmpty(maxFileSize))
            {
                return new ValidationResult("Config error, File Size is not available");
            }

            sizeInByte = StringHelper.GetSizeInBytesFromString(maxFileSize);

            if (sizeInByte is null)
            {
                return new ValidationResult("Config Error, Measurement is invalid");
            }

            var file = value as IFormFile;

            if (file == null)
            {
                return new ValidationResult("Invalid file");
            }

            if (file.Length > sizeInByte)
            {
                return new ValidationResult("Max file size exceeded");
            }

            return ValidationResult.Success!;
        }
    }
}