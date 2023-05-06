// <copyright file="CsvInputFormatter.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;

namespace OnlineSales.Formatters.Csv;

public class CsvInputFormatter : InputFormatter
{
    public CsvInputFormatter()
    {
        SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("text/csv"));
    }

    public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context)
    {
        var type = context.ModelType;
        var request = context.HttpContext.Request;
        MediaTypeHeaderValue.TryParse(request.ContentType, out var requestContentType);

        var result = await ReadStreamAsync(type, request.Body);
        return await InputFormatterResult.SuccessAsync(result);
    }

    protected override bool CanReadType(Type type)
    {
        return IsTypeOfIEnumerable(type);
    }

    protected async Task<object> ReadStreamAsync(Type type, Stream stream)
    {
        Type? itemType;
        var typeIsArray = false;
        IList? list;

        if (type.GetGenericArguments().Length > 0)
        {
            itemType = type.GetGenericArguments()[0];
            list = Activator.CreateInstance(type) as IList;
        }
        else
        {
            typeIsArray = true;
            itemType = type.GetElementType();

            var listType = typeof(List<>);
            var constructedListType = listType.MakeGenericType(itemType!);

            list = Activator.CreateInstance(constructedListType) as IList;
        }

        var itemTypeInGeneric = list!.GetType().GetTypeInfo().GenericTypeArguments[0];

        var reader = new StreamReader(stream, Encoding.Default);

        using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
        {
            csv.Context.TypeConverterCache.AddConverter<string?>(new NullableStringConverter());
            csv.Context.TypeConverterCache.RemoveConverter<DateTime?>();
            csv.Context.TypeConverterCache.AddConverter<DateTime?>(new NullableDateTimeToUtcConverter(typeof(DateTime?), csv.Context.TypeConverterCache));

            csv.Context.RegisterCamelCaseClassMap(itemType!);

            await foreach (var record in csv.GetRecordsAsync(itemTypeInGeneric))
            {
                list.Add(record);
            }
        }

        if (typeIsArray)
        {
            var array = Array.CreateInstance(itemType!, list!.Count);

            for (var t = 0; t < list.Count; t++)
            {
                array.SetValue(list[t], t);
            }

            return array;
        }

        return list!;
    }

    private bool IsTypeOfIEnumerable(Type type)
    {
        foreach (var interfaceType in type.GetInterfaces())
        {
            if (interfaceType == typeof(IList))
            {
                return true;
            }
        }

        return false;
    }
}

public class NullableStringConverter : DefaultTypeConverter
{
    public override object? ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData)
    {
        if (string.IsNullOrEmpty(text))
        {
            return null;
        }

        return text;
    }
}

public class NullableDateTimeToUtcConverter : NullableConverter
{
    public NullableDateTimeToUtcConverter(Type type, TypeConverterCache typeConverterFactory)
        : base(type, typeConverterFactory)
    {
    }

    public override object? ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData)
    {
        var parsedDate = base.ConvertFromString(text, row, memberMapData);

        if (parsedDate is not null)
        {
            return ((DateTime)parsedDate).ToUniversalTime();
        }
        else
        {
            return null;
        }
    }
}