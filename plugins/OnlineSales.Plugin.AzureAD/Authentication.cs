// <copyright file="Authentication.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;

namespace OnlineSales.Plugin.AzureAD
{
    public static class Authentication
    {
        private static string defaultAuthScheme = "ApiAppAuthentication";
        private static string appAuthScheme = "WebAppAuthentication";
        private static string apiAuthScheme = "WebApiAuthentication";

        public static void ConfigureAuth(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthentication(defaultAuthScheme)
             .AddPolicyScheme(defaultAuthScheme, defaultAuthScheme, opts =>
             {
                 opts.ForwardDefaultSelector = ctx =>
                 {
                     var authorizationFromCookie = ctx.Request.Cookies[".AspNetCore.Cookies"];
                     if (!string.IsNullOrEmpty(authorizationFromCookie))
                     {
                         return "Cookies";
                     }

                     var authorizationFromQuery = ctx.Request.Query["authorization"].FirstOrDefault();
                     if (!string.IsNullOrEmpty(authorizationFromQuery))
                     {
                         ctx.Request.Headers.Authorization = $"Bearer {authorizationFromQuery}";
                     }

                     var authorization = ctx.Request.Headers.Authorization.FirstOrDefault();
                     if (!string.IsNullOrEmpty(authorization) && authorization.StartsWith("Bearer "))
                     {
                         var token = authorization.Substring("Bearer ".Length).Trim();
                         var jwtHandler = new JwtSecurityTokenHandler();

                         return jwtHandler.CanReadToken(token) ? apiAuthScheme : appAuthScheme;
                     }

                     return appAuthScheme;
                 };
             })
             .AddMicrosoftIdentityWebApi(
                 jwtOptions =>
                 {
                     jwtOptions.Events = new JwtBearerEventsHandler();
                 }, identityOptions =>
                 {
                     identityOptions.Instance = configuration.GetValue<string>("AzureAD:Instance") ?? string.Empty;
                     identityOptions.TenantId = configuration.GetValue<string>("AzureAD:TenantId") ?? string.Empty;
                     identityOptions.Domain = configuration.GetValue<string>("AzureAD:Domain") ?? string.Empty;
                     identityOptions.ClientId = configuration.GetValue<string>("AzureAD:ClientId") ?? string.Empty;
                     identityOptions.ClientSecret = configuration.GetValue<string>("AzureAD:ClientSecret") ?? string.Empty;
                 }, jwtBearerScheme: apiAuthScheme);
            services.AddAuthentication()
                        .AddMicrosoftIdentityWebApp(configuration, openIdConnectScheme: appAuthScheme);

            services.AddRazorPages().AddMvcOptions(options =>
            {
                var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
                options.Filters.Add(new AuthorizeFilter(policy));
            }).AddMicrosoftIdentityUI();
        }
    }
}
