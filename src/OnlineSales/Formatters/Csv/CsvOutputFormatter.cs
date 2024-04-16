// <copyright file="CsvOutputFormatter.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Collections;
using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;

namespace OnlineSales.Formatters.Csv;

public class CsvOutputFormatter : OutputFormatter
{
    public CsvOutputFormatter()
    {
        ContentType = "text/csv";
        SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("text/csv"));
    }

    public string ContentType { get; private set; }

    public async override Task WriteResponseBodyAsync(OutputFormatterWriteContext context)
    {
        var response = context.HttpContext.Response;

        var type = context.Object!.GetType();
        Type itemType;

        if (type.GetGenericArguments().Length > 0)
        {
            itemType = type.GetGenericArguments()[0];
        }
        else
        {
            itemType = type.GetElementType()!;
        }

        var streamWriter = new StreamWriter(response.Body, Encoding.Default);

        await using (var csv = new CsvWriter(streamWriter, CultureInfo.InvariantCulture))
        {
            csv.Context.TypeConverterCache.AddConverter<DateTime>(new JsonStyleDateTimeConverter());
            csv.Context.RegisterCamelCaseClassMap(itemType!);

            await csv.WriteRecordsAsync(context.Object as IEnumerable);
        }
    }

    protected override bool CanWriteType(Type? type)
    {
        if (type == null)
        {
            throw new ArgumentNullException(nameof(type));
        }

        return IsTypeOfIEnumerable(type);
    }

    private bool IsTypeOfIEnumerable(Type type)
    {
        return typeof(IEnumerable).IsAssignableFrom(type);
    }
}

public class JsonStyleDateTimeConverter : ITypeConverter
{
    public string? ConvertToString(object? value, IWriterRow row, MemberMapData memberMapData)
    {
        if (value is DateTime dateTime)
        {
            return dateTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
        }

        return string.Empty;
    }

    public object? ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData)
    {
        if (DateTime.TryParse(text, out var result))
        {
            return result;
        }

        return null;
    }
}