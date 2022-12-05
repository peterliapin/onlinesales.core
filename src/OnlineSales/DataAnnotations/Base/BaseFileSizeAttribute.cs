// <copyright file="BaseFileSizeAttribute.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel.DataAnnotations;

namespace OnlineSales.DataAnnotations.Base
{
    public class BaseFileSizeAttribute : ValidationAttribute
    {
        public string MaxFileSize { get; set; } = string.Empty;

        protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
        {
            long sizeInByte;
            int size;

            var file = value as IFormFile;

            if (file == null)
            {
                return ValidationResult.Success!;
            }

            string measurement = MaxFileSize![^2..];
            string fileSize = MaxFileSize![0..^2];

            if (!measurement.All(char.IsLetter))
            {
                measurement = MaxFileSize![^1..];
                fileSize = MaxFileSize![0..^1];
            }

            if (!int.TryParse(fileSize, out size))
            {
                return new ValidationResult("Config error, File Size is invalid");
            }

            if (measurement.ToUpper().Equals("MB"))
            {
                sizeInByte = size * 1024 * 1024;
            }
            else if (measurement.ToUpper().Equals("KB"))
            {
                sizeInByte = size * 1024;
            }
            else if (measurement.ToUpper().Equals("B"))
            {
                sizeInByte = size;
            }
            else
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
