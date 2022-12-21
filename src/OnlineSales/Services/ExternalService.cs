// <copyright file="ExternalService.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Nest;
using OnlineSales.Configuration;
using OnlineSales.DTOs;

namespace OnlineSales.Services
{
    public class ExternalService
    {
        private readonly IOptions<IpConfig> options;

        public ExternalService(IOptions<IpConfig> options)
        {
            this.options = options;
        }

        public async Task<IPDetailDto?> GetIPDetail(string ip)
        {
            IPDetailDto? ipDetailsDto;

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
                ipDetailsDto = JsonSerializer.Deserialize<IPDetailDto>(response.Content.ReadAsStringAsync().Result);

                Log.Information("Success of resolving {0}", ipDetailsDto!.IP!);
            }
            else
            {
                var responseContent = await response.Content.ReadAsStringAsync();

                throw new IPDetailException(responseContent);
            }

            return ipDetailsDto;
        }
    }
}
