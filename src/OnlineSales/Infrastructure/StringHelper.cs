// <copyright file="StringHelper.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Text.RegularExpressions;

namespace OnlineSales.Infrastructure;

public static class StringHelper
{
    public static long? GetSizeFromString(string size)
    {
        long? convertedSize = null;
        string pattern = @"(\d+)(\D+)";

        var match = Regex.Match(size, pattern);

        if (match.Success)
        {
            long value = long.Parse(match.Groups[1].Value);
            string unit = match.Groups[2].Value;

            switch (unit.ToUpper())
            {
                case "MB":
                    convertedSize = value * 1024 * 1024;
                    break;
                case "KB":
                    convertedSize = value * 1024;
                    break;
                case "B":
                    convertedSize = value;
                    break;
                default:
                    break;
            }
        }

        return convertedSize;
    }
}
