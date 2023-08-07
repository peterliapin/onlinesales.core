// <copyright file="LanguageCodeAttribute.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;

namespace OnlineSales.DataAnnotations;

/// <summary>
/// Trust a property value must be like a 4 letter language string.
/// </summary>
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

        if (string.IsNullOrEmpty(languageCode) || !Regex.IsMatch(languageCode, "[a-z]{2}-[A-Z]{2}"))
        {
            return new ValidationResult($"Language code '{languageCode}' is not match '[a-z]{{2}}-[A-Z]{{2}}' format.");
        }

        return CultureInfo.GetCultureInfo(languageCode!, true) == null
                ? new ValidationResult("Culture not found")
                : ValidationResult.Success;
    }
}