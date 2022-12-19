// <copyright file="CurrencyCodeAttribute.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace OnlineSales.DataAnnotations;

public class CurrencyCodeAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
    {
        var currencyCode = value as string;

        if (currencyCode == null)
        {
            return new ValidationResult("Invalid Currency Code");
        }

        var symbol = CultureInfo.GetCultures(CultureTypes.AllCultures).Where(c => !c.IsNeutralCulture).Select(culture =>
        {
            try
            {
                return new RegionInfo(culture.Name);
            }
            catch
            {
                return null;
            }
        }).Where(ri => ri != null && ri.ISOCurrencySymbol == currencyCode.ToUpper()).Select(ri => ri!.CurrencySymbol).FirstOrDefault();

        if (symbol != null)
        {
            return ValidationResult.Success!;
        }
        else
        {
            return new ValidationResult("Invalid Currency Code");
        }
    }
}