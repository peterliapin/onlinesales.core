// <copyright file="StringHelper.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Text.RegularExpressions;

namespace OnlineSales.Infrastructure;

public static class StringHelper
{
    /// <summary>
    /// Calculate a size in bytes using a given string based size with unit.
    /// </summary>
    /// <param name="size">String based size. Supported units: MB,KB,B. Examples: 10MB,100KB,1024B.</param>
    /// <returns>Size in Bytes.</returns>
    public static long? GetSizeInBytesFromString(string size)
    {
        long? convertedSize = null;
        var pattern = @"(\d+)(\D+)";

        var match = Regex.Match(size, pattern);

        if (match.Success)
        {
            var value = long.Parse(match.Groups[1].Value);
            var unit = match.Groups[2].Value;

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