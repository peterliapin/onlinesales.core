// <copyright file="CsvClassMapHelper.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Globalization;
using System.Text.Json;
using CsvHelper;
using CsvHelper.Configuration;

namespace OnlineSales.Formatters.Csv;

public static class CsvClassMapHelper
{
    private static readonly List<string> OptionalAttributes = new List<string>
    {
        "Id",
        "CreatedAt",
        "UpdatedAt",
    };

    public static void RegisterCamelCaseClassMapWithOptionalAttributes(this CsvContext csvContext, Type itemType)
    {
        var mapType = typeof(DefaultClassMap<>);
        var constructedMapType = mapType.MakeGenericType(itemType!);

        var map = (ClassMap)Activator.CreateInstance(constructedMapType) !;
        map.AutoMap(CultureInfo.InvariantCulture);

        foreach (var memberMapData in map.MemberMaps.Select(m => m.Data))
        {
            if (OptionalAttributes.Contains(memberMapData.Member!.Name))
            {
                memberMapData.IsOptional = true;
            }

            memberMapData.Names.Add(JsonNamingPolicy.CamelCase.ConvertName(memberMapData.Member!.Name));
        }

        csvContext.RegisterClassMap(map);
    }
}