// <copyright file="AccountExternalService.cs" company="WavePoint Co. Ltd.">
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
    public class AccountExternalService : IAccountExternalService
    {
        protected static readonly JsonSerializerOptions SerializeOptions = new JsonSerializerOptions();
        private readonly IOptions<AccountDetailsApiConfig> accountDetailsApiConfig;

        public AccountExternalService(IOptions<AccountDetailsApiConfig> accountDetailsApiConfig)
        {
            this.accountDetailsApiConfig = accountDetailsApiConfig;

            if (SerializeOptions.PropertyNamingPolicy == null)
            {
                JsonHelper.Configure(SerializeOptions, new CityNamingPolicy());
            }
        }

        public async Task<AccountDetailsInfo?> GetAccountDetails(string domain)
        {
            var apiUrl = this.accountDetailsApiConfig.Value.Url;
            var accessToken = this.accountDetailsApiConfig.Value.ApiKey;

            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);

            var queryParams = new Dictionary<string, string>
            {
                ["url"] = "https://" + domain!,
            };

            var response = await client.PostAsync(QueryHelpers.AddQueryString(apiUrl, queryParams!), null);

            if (response.IsSuccessStatusCode)
            {
                var jsonDoc = JsonDocument.Parse(response.Content.ReadAsStringAsync().Result);
                var apiSuccess = Convert.ToBoolean(jsonDoc.RootElement.GetProperty("success").ValueKind.ToString());
                if (apiSuccess)
                {
                    AccountDetailsInfo? companybasicDetails = null;

                    if (jsonDoc.RootElement.TryGetProperty("objects", out var objects) && objects.TryGetProperty("company", out var company))
                    {
                        try
                        {
                            companybasicDetails = JsonSerializer.Deserialize<AccountDetailsInfo>(company, SerializeOptions);
                        }
                        catch (Exception ex)
                        {
                            Log.Error("Cannot deserialize AccountDetailsInfo. Reason: " + ex.Message);
                        }
                    }

                    if (companybasicDetails == null)
                    {
                        return null;
                    }

                    Dictionary<string, string>? socialsDecoded = null;

                    if (jsonDoc.RootElement.TryGetProperty("domain", out var accountDomain) && accountDomain.TryGetProperty("social_media", out var socials))
                    {
                        try
                        {
                            socialsDecoded = JsonSerializer.Deserialize<Dictionary<string, string>>(socials, SerializeOptions);

                            if (socials.ValueKind != JsonValueKind.Null)
                            {
                                var socialsNullRemoved = socialsDecoded!.Where(f => f.Value != null).ToDictionary(x => x.Key, x => x.Value);

                                companybasicDetails!.SocialMedia = socialsNullRemoved;
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Error("Cannot deserialize AccountDetailsInfo. Reason: " + ex.Message);
                        }
                    }

                    companybasicDetails!.Data = jsonDoc.RootElement.ToString();

                    return companybasicDetails;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        private sealed class CityNamingPolicy : JsonNamingPolicy
        {
            public override string ConvertName(string name)
            {
                if (name == "CityName")
                {
                    return "city";
                }
                else
                {
                    return JsonNamingPolicy.CamelCase.ConvertName(name);
                }
            }
        }
    }
}
