// <copyright file="EnumHelper.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel;
using System.Reflection;

namespace OnlineSales.Helpers;

public static class EnumHelper
{
    public static Dictionary<string, string> GetEnumDescriptions<TEnum>()
        where TEnum : Enum
    {
        var descriptions = new Dictionary<string, string>();
        foreach (TEnum value in Enum.GetValues(typeof(TEnum)))
        {
            var descriptionAttribute = value.GetType().GetField(value.ToString())!
                .GetCustomAttribute<DescriptionAttribute>();

            var description = descriptionAttribute?.Description ?? value.ToString();
            descriptions[value.ToString()] = description;
        }

        return descriptions;
    }
}