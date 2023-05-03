// <copyright file="LanguageCodeAttribute.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Globalization;

namespace OnlineSales.DataAnnotations;

public class LanguageCodeAttribute : ValidationAttribute
{
    private readonly bool nullAllowed;

    public LanguageCodeAttribute(bool nullAllowed = false)
    {
        this.nullAllowed = nullAllowed;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (nullAllowed && value == null)
        {
            return ValidationResult.Success;
        }

        var languageCode = value as string;

        if (CultureInfo.GetCultures(CultureTypes.AllCultures).FirstOrDefault(culture => culture.Name == languageCode) == null)
        {
            return new ValidationResult("Culture not found");
        }

        return ValidationResult.Success;
    }
}
