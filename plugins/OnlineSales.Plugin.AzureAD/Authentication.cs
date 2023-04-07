// <copyright file="Authentication.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.Cookies;
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
        // Selects between App/Api schemes. Look into line 23.  if cookies available authorize using them if not - try to use API, if bearer not avaiable try to perform APP authentication 
        public const string ApiAppAuthScheme = "ApiAppAuthentication";
        public const string AppAuthScheme = "WebAppAuthentication";
        public const string ApiAuthScheme = "WebApiAuthentication";
        private static string cookieName = "auth_ticket";

        public static void ConfigureAuth(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthentication(ApiAppAuthScheme)
             .AddPolicyScheme(ApiAppAuthScheme, ApiAppAuthScheme, opts =>
             {
                 opts.ForwardDefaultSelector = ctx =>
                 {
                     var authorizationFromCookie = ctx.Request.Cookies[cookieName];
                     if (!string.IsNullOrEmpty(authorizationFromCookie))
                     {
                         return CookieAuthenticationDefaults.AuthenticationScheme;
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

                         return jwtHandler.CanReadToken(token) ? ApiAuthScheme : AppAuthScheme;
                     }

                     return AppAuthScheme;
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
                 }, jwtBearerScheme: ApiAuthScheme);
            services.AddAuthentication(ApiAuthScheme)
                        .AddMicrosoftIdentityWebApp(configuration, openIdConnectScheme: AppAuthScheme);
            services.Configure<CookieAuthenticationOptions>(CookieAuthenticationDefaults.AuthenticationScheme, options =>
            {
                options.Cookie.Name = cookieName;
            });

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
