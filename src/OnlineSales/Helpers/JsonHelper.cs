// <copyright file="JsonHelper.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using CsvHelper.TypeConversion;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using OnlineSales.Configuration;

namespace OnlineSales.Helpers;

public enum JsonNamingConvention
{
    CamelCase,
    SnakeCase,
}

public class JsonHelper
{
    protected static readonly JsonSerializerOptions SerializeOptions = new JsonSerializerOptions();

    static JsonHelper()
    {
        Configure(SerializeOptions);
    }

    public static void Configure(JsonSerializerOptions options, JsonNamingPolicy policy)
    {
        options.PropertyNamingPolicy = policy;

        options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault;
        options.Converters.Add(new JsonStringEnumConverter());
    }

    public static void Configure(JsonSerializerOptions options, JsonNamingConvention convention = JsonNamingConvention.CamelCase)
    {
        if (convention == JsonNamingConvention.CamelCase)
        {
            options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        }
        else
        {
            options.PropertyNamingPolicy = SnakeCaseNamingPolicy.Instance;
        }

        options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault;
        options.Converters.Add(new JsonStringEnumConverter());
        options.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    }

    public static string Serialize(object obj)
    {
        return JsonSerializer.Serialize(obj, SerializeOptions);
    }

    public static T? Deserialize<T>(string data)
    {
        if (string.IsNullOrEmpty(data))
        {
            return default(T);
        }
        else
        {
            return JsonSerializer.Deserialize<T>(data, SerializeOptions);
        }
    }

    public class BooleanConverter : JsonConverter<bool>
    {
        public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.True:
                    return true;
                case JsonTokenType.False:
                    return false;
                case JsonTokenType.String:
                    return reader.GetString() switch
                    {
                        "true" => true,
                        "false" => false,
                        _ => throw new JsonException()
                    };
                default:
                    throw new JsonException();
            }
        }

        public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options)
        {
            writer.WriteBooleanValue(value);
        }
    }
}