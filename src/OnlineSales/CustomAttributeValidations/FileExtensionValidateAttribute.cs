// <copyright file="FileExtensionValidateAttribute.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel.DataAnnotations;

namespace OnlineSales.CustomAttributeValidations
{
    public class FileExtensionValidateAttribute : ValidationAttribute
    {
        private readonly string validExtensions;

        public FileExtensionValidateAttribute(string validExtensions)
        {
            this.validExtensions = validExtensions;
        }

        public override bool IsValid(object? value)
        {
            bool ok = false;

            var extString = validExtensions;
            var listOfExt = extString.Split(';').ToList();

            var file = value as IFormFile;

            if (file == null)
            {
                return true;
            }

            string currentExt = Path.GetExtension(file.FileName.ToLower());

            var hasMatchingExt = from ext in listOfExt! where ext == currentExt select ext;

            if (hasMatchingExt.Any())
            {
                ok = true;
            }
            else
            {
                ok = false;
            }

            return ok;
        }
    }
}
