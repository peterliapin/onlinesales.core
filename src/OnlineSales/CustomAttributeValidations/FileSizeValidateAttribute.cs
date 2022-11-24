// <copyright file="FileSizeValidateAttribute.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel.DataAnnotations;

namespace OnlineSales.CustomValidations
{
    public class FileSizeValidateAttribute : ValidationAttribute
    {
        private readonly long maxFileSizeInBytes;

        public FileSizeValidateAttribute(long maxFileSizeInBytes)
        {
            this.maxFileSizeInBytes = maxFileSizeInBytes;
        }

        public override bool IsValid(object? value)
        {
            bool ok = true;

            var file = value as IFormFile;

            if (file == null)
            {
                return true;
            }

            if (file.Length > maxFileSizeInBytes)
            {
                ok = false;
            }

            return ok;
        }
    }
}
