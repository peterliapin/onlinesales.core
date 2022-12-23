// <copyright file="IpDetailsService.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using OnlineSales.Configuration;
using OnlineSales.DTOs;

namespace OnlineSales.Services;

public class IpDetailsService
{
    protected static readonly JsonSerializerOptions SerializeOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = SnakeCaseNamingPolicy.Instance,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
    };

    private readonly IOptions<IpConfig> options;

    public IpDetailsService(IOptions<IpConfig> options)
    {
        this.options = options;
        SerializeOptions.Converters.Add(new JsonStringEnumConverter());
    }

    public async Task<IPDetailsDto?> GetIPDetail(string ip)
    {
        IPDetailsDto? ipDetailsDto;

        var client = new HttpClient();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var queryParams = new Dictionary<string, string>
        {
            ["apiKey"] = options.Value.AuthKey,
            ["ip"] = ip,
        };

        var response = await client.GetAsync(QueryHelpers.AddQueryString(options.Value.Url, queryParams!));

        if (response.IsSuccessStatusCode)
        {
            ipDetailsDto = JsonSerializer.Deserialize<IPDetailsDto>(response.Content.ReadAsStringAsync().Result, SerializeOptions);

            Log.Information("Success of resolving {0}", ipDetailsDto!.Ip!);
        }
        else
        {
            var responseContent = await response.Content.ReadAsStringAsync();

            throw new IPDetailsException(responseContent);
        }

        return ipDetailsDto;
    }
}
