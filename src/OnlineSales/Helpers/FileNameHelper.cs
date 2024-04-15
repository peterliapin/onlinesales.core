// <copyright file="FileNameHelper.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Unidecode.NET;

namespace OnlineSales.Helpers;

public static class FileNameHelper
{
    public static string Slugify(this string input)
    {
        return input.RemoveWhitespace().RemoveDeniedCharacters().RemoveDiacritics().ToLower();
    }

    public static string ToTranslit(this string input)
    {
        return input.Unidecode();
    }

    private static string RemoveWhitespace(this string input)
    {
        return Regex.Replace(input, @"\s", " ");
    }

    private static string RemoveDeniedCharacters(this string input)
    {
        return Regex.Replace(input, @"[^a-zA-Z0-9\-\._]", string.Empty);
    }

    private static string RemoveDiacritics(this string text)
    {
        var normalizedString = text.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder(capacity: normalizedString.Length);

        foreach(var c in normalizedString)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }

        return stringBuilder
            .ToString()
            .Normalize(NormalizationForm.FormC);
    }
}
