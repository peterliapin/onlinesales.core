// <copyright file="EmailValidationExternalService.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using OnlineSales.Configuration;
using OnlineSales.DTOs;
using OnlineSales.Helpers;
using OnlineSales.Interfaces;

namespace OnlineSales.Services
{
    public class EmailValidationExternalService : IEmailValidationExternalService
    {
        protected static readonly JsonSerializerOptions SerializeOptions = new JsonSerializerOptions();
        private readonly IOptions<EmailVerificationApiConfig> emailVerificationApiConfig;

        public EmailValidationExternalService(IOptions<EmailVerificationApiConfig> emailVerificationApiConfig)
        {
            this.emailVerificationApiConfig = emailVerificationApiConfig;

            if (SerializeOptions.PropertyNamingPolicy == null)
            {
                JsonHelper.Configure(SerializeOptions, JsonNamingConvention.CamelCase);
            }
        }

        public async Task<EmailVerifyInfoDto> Validate(string email)
        {
            var apiUrl = emailVerificationApiConfig.Value.Url;
            var apiKey = emailVerificationApiConfig.Value.ApiKey;

            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var queryParams = new Dictionary<string, string>
            {
                ["apiKey"] = apiKey,
                ["emailAddress"] = email,
            };

            var response = await client.GetAsync(QueryHelpers.AddQueryString(apiUrl, queryParams!));

            if (response.IsSuccessStatusCode)
            {
                var emailVerify = JsonSerializer.Deserialize<EmailVerifyInfoDto>(response.Content.ReadAsStringAsync().Result, SerializeOptions);

                Log.Information("Success of resolving {0}", emailVerify!.EmailAddress!);

                return emailVerify;
            }
            else
            {
                var responseContent = await response.Content.ReadAsStringAsync();

                throw new KeyNotFoundException(responseContent);
            }
        }
    }
}
