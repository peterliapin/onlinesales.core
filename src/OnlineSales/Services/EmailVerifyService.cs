// <copyright file="EmailVerifyService.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OnlineSales.Configuration;
using OnlineSales.Data;
using OnlineSales.DTOs;
using OnlineSales.Entities;
using OnlineSales.Helpers;
using OnlineSales.Interfaces;

namespace OnlineSales.Services
{
    public class EmailVerifyService : IEmailVerifyService
    {
        protected static readonly JsonSerializerOptions SerializeOptions = new JsonSerializerOptions();
        private readonly ApiDbContext apiDbContext;
        private readonly IDomainService domainService;
        private readonly IOptions<EmailVerificationApiConfig> emailVerificationApiConfig;

        public EmailVerifyService(ApiDbContext apiDbContext, IDomainService domainService, IOptions<EmailVerificationApiConfig> emailVerificationApiConfig)
        {
            this.apiDbContext = apiDbContext;
            this.domainService = domainService;
            this.emailVerificationApiConfig = emailVerificationApiConfig;

            if (SerializeOptions.PropertyNamingPolicy == null)
            {
                JsonHelper.Configure(SerializeOptions, JsonNamingConvention.CamelCase); 
            }
        }

        public async Task<Domain> Validate(string email)
        {
            var splittedEmail = email.Split("@");
            var domain = splittedEmail.Last().ToString();
            
            var domainExistance = await GetDomainData(domain);

            if (domainExistance.DomainData != null)
            {
                return domainExistance.DomainData;
            }
            else
            {
                Domain newDomain = new Domain()
                {
                    Name = domain,
                };

                if (!domainExistance.hasDomain)
                {
                    apiDbContext.Add(newDomain);
                }

                await domainService.Verify(newDomain!);
                await VerifyEmail(email, newDomain);
                await apiDbContext.SaveChangesAsync();

                return newDomain;
            }
        }

        public async Task<(bool hasDomain, Domain? DomainData)> GetDomainData(string domain)
        {
            bool hasDomain = false;

            var domainData = await (from dbDomain in apiDbContext.Domains where dbDomain.Name == domain select dbDomain).FirstOrDefaultAsync();

            if (domainData != null)
            {
                hasDomain = true;
            }

            if (domainData != null && domainData!.Free != null && domainData!.Disposable != null && domainData!.DnsRecords != null)
            {
                return (hasDomain, domainData);
            }

            return (hasDomain, null);
        }

        public async Task VerifyEmail(string email, Domain domainRecord)
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
                bool freeCheck = false;
                bool disposableCheck = false;
                bool catchAllCheck = false;

                var emailVerify = JsonSerializer.Deserialize<EmailVerifyInfoDto>(response.Content.ReadAsStringAsync().Result, SerializeOptions);

                if (!bool.TryParse(emailVerify!.FreeCheck, out freeCheck) || !bool.TryParse(emailVerify!.DisposableCheck, out disposableCheck) || !bool.TryParse(emailVerify!.CatchAllCheck, out catchAllCheck))
                {
                    throw new KeyNotFoundException("Some values are not found for email validation");
                }

                domainRecord.Free = freeCheck;
                domainRecord.Disposable = disposableCheck;
                domainRecord.CatchAll = catchAllCheck;

                Log.Information("Success of resolving {0}", emailVerify!.EmailAddress!);
            }
            else
            {
                var responseContent = await response.Content.ReadAsStringAsync();

                throw new KeyNotFoundException(responseContent);
            }
        }
    }
}
