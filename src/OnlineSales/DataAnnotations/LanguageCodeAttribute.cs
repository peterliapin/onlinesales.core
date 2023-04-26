// <copyright file="LanguageCodeAttribute.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace OnlineSales.DataAnnotations;

public class LanguageCodeAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var languageCode = value as string;

        if (string.IsNullOrEmpty(languageCode))
        {
            return new ValidationResult("Invalid language code");
        }

        if (CultureInfo.GetCultures(CultureTypes.AllCultures).FirstOrDefault(culture => culture.Name == languageCode) == null)
        {
            return new ValidationResult("Culture not found");
        }

        return ValidationResult.Success;
    }
}
