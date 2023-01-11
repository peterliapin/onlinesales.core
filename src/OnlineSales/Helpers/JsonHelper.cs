// <copyright file="JsonHelper.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Text.Json;
using System.Text.Json.Serialization;
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
}