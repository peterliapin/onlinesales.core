// <copyright file="CsvOutputFormatter.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Collections;
using System.Globalization;
using System.Text;
using CsvHelper;
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

        Type type = context.Object!.GetType();
        Type itemType;

        if (type.GetGenericArguments().Length > 0)
        {
            itemType = type.GetGenericArguments()[0];
        }
        else
        {
            itemType = type.GetElementType() !;
        }

        var streamWriter = new StreamWriter(response.Body, Encoding.Default);

        await using (var csv = new CsvWriter(streamWriter, CultureInfo.InvariantCulture))
        {
            csv.Context.RegisterCamelCaseClassMap(itemType!);

            await csv.WriteRecordsAsync(context.Object as IEnumerable);
        }
    }

    protected override bool CanWriteType(Type? type)
    {
        if (type == null)
        {
            throw new ArgumentNullException("type");
        }

        return IsTypeOfIEnumerable(type);
    }

    private bool IsTypeOfIEnumerable(Type type)
    {
        return typeof(IEnumerable).IsAssignableFrom(type);
    }
}
