// <copyright file="Authentication.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;

namespace OnlineSales.Plugin.AzureAD
{
    public static class Authentication
    {
        public const string ApiAuthScheme = "WebApiAuthentication";

        public static void ConfigureAuth(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthentication()
             .AddMicrosoftIdentityWebApi(
                 jwtOptions =>
                 {
                 }, identityOptions =>
                 {
                     identityOptions.Instance = configuration.GetValue<string>("AzureAD:Instance") ?? string.Empty;
                     identityOptions.TenantId = configuration.GetValue<string>("AzureAD:TenantId") ?? string.Empty;
                     identityOptions.Domain = configuration.GetValue<string>("AzureAD:Domain") ?? string.Empty;
                     identityOptions.ClientId = configuration.GetValue<string>("AzureAD:ClientId") ?? string.Empty;
                     identityOptions.ClientSecret = configuration.GetValue<string>("AzureAD:ClientSecret") ?? string.Empty;
                 });
            services.AddTransient<JwtBearerHandler>((provider) =>
            {
                var options = provider.GetService<IOptionsMonitor<JwtBearerOptions>>();
                var lfac = provider.GetService<ILoggerFactory>();
                var urle = provider.GetService<UrlEncoder>();
                var clock = provider.GetService<ISystemClock>();

                var z = new JwtBearerHandler(options!, lfac!, urle!, clock!);
                return z;
            });
        }
    }
}
