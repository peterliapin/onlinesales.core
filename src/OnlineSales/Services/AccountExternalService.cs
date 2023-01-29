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
                JsonHelper.Configure(SerializeOptions, JsonNamingConvention.CamelCase);
            }
        }

        public async Task<AccountDetailsInfo> GetAccountDetails(string domain)
        {
            var apiUrl = accountDetailsApiConfig.Value.Url;
            var accessToken = accountDetailsApiConfig.Value.ApiKey;

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
                    var company = jsonDoc.RootElement.GetProperty("objects").GetProperty("company");

                    var socials = jsonDoc.RootElement.GetProperty("domain").GetProperty("social_media");

                    var companybasicDetails = JsonSerializer.Deserialize<AccountDetailsInfo>(company, SerializeOptions);

                    if (companybasicDetails!.Name == null)
                    {
                        return new AccountDetailsInfo()
                        {
                            Name = domain,
                        };
                    }

                    var socialsDecoded = JsonSerializer.Deserialize<Dictionary<string, string>>(socials, SerializeOptions);

                    if (socials.ValueKind != JsonValueKind.Null)
                    {
                        var socialsNullRemoved = socialsDecoded!.Where(f => f.Value != null).ToDictionary(x => x.Key, x => x.Value);

                        companybasicDetails!.SocialMedia = socialsNullRemoved;
                    }

                    companybasicDetails!.Data = jsonDoc.RootElement.ToString();

                    companybasicDetails.AccountSynced = true;

                    return companybasicDetails;
                }
                else
                {
                    return new AccountDetailsInfo()
                    {
                        Name = domain,
                        AccountSynced = false,
                    };
                }
            }
            else
            {
                return new AccountDetailsInfo()
                {
                    Name = domain,
                    AccountSynced = false,
                };
            }
        }
    }
}
