// <copyright file="CurrencyCodeAttribute.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text;
using Npgsql.Internal.TypeHandlers.NetworkHandlers;

namespace OnlineSales.DataAnnotations;

public class CurrencyCodeAttribute : ValidationAttribute
{
    public static string GetAllCurrencyCodesRegex()
    {
        var sb = new StringBuilder("^(");

        CultureInfo.GetCultures(CultureTypes.AllCultures).Where(c => !c.IsNeutralCulture).ToList().ForEach(c =>
        {
            try
            {
                var ri = new RegionInfo(c.Name);
                sb.Append(ri.ISOCurrencySymbol);
                sb.Append("|");
            }
            catch
            {
                Log.Error("Cannot get CurrencySymbol for culture. CultureName: " + c.Name);
            }
        });

        sb.Remove(sb.Length - 1, 1);
        sb.Append(")$");

        return sb.ToString();
    }

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