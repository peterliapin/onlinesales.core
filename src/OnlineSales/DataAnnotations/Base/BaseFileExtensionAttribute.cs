// <copyright file="BaseFileExtensionAttribute.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel.DataAnnotations;

namespace OnlineSales.DataAnnotations.Base
{
    public class BaseFileExtensionAttribute : ValidationAttribute
    {
        public string[] ListOfExt { get; set; } = new string[0];

        protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
        {
            var file = value as IFormFile;

            if (file == null)
            {
                return ValidationResult.Success!; // Required should handel this senario.
            }

            string currentExt = Path.GetExtension(file.FileName.ToLower());

            var hasMatchingExt = from ext in ListOfExt! where ext == currentExt select ext;

            if (!hasMatchingExt.Any())
            {
                return new ValidationResult("Invalid file extension");
            }

            return ValidationResult.Success!;
        }
    }
}