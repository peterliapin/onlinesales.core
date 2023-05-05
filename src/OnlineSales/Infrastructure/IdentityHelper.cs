// <copyright file="IdentityHelper.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Identity.Web;
using OnlineSales.Data;
using OnlineSales.Entities;
using OnlineSales.Identity;

namespace OnlineSales.Infrastructure
{
    public static class IdentityHelper
    {
        public static void ConfigureIdentity(WebApplicationBuilder builder)
        {
            builder.Services.AddIdentity<User, IdentityRole>()
                .AddEntityFrameworkStores<PgDbContext>();
            builder.Services.ConfigureApplicationCookie(options =>
            {
                // Cookie settings
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromHours(12);
                options.Cookie.Name = "auth_ticket";

                options.LoginPath = "/api/Identity/external-login";
                options.AccessDeniedPath = "/access-denied";
                options.SlidingExpiration = true;
            });

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddMicrosoftIdentityWebApi(
                 jwtOptions =>
                 {
                     jwtOptions.Events = new AzureAdJwtBearerEventsHandler();
                 }, identityOptions =>
                 {
                     ConfigureIdentityAuthOptions(builder, identityOptions);
                 });
            builder.Services.AddAuthentication().AddMicrosoftIdentityWebApp(
                identityOptions =>
                {
                    ConfigureIdentityAuthOptions(builder, identityOptions);
                    identityOptions.CallbackPath = "/api/identity/callback";
                    identityOptions.SkipUnrecognizedRequests = true;
                }, cookieOptions =>
                {
                    cookieOptions.Cookie.Name = "auth_ticket";
                    cookieOptions.Events = new AzureAdCookieEventsHandler();
                });
            builder.Services.Configure<CookiePolicyOptions>(options =>
            {
                options.MinimumSameSitePolicy = SameSiteMode.None;
                options.Secure = CookieSecurePolicy.Always;
            });
        }

        public static async Task<ClaimsPrincipal> TryLoginOnRegister(SignInManager<User> signInManager, UserManager<User> userManager, string userEmail, string authProvider)
        {
            var user = await userManager.FindByEmailAsync(userEmail);
            if (user == null)
            {
                user = new User
                {
                    UserName = userEmail,
                    Email = userEmail,
                    CreatedAt = DateTime.UtcNow,
                    DisplayName = userEmail,
                };
                await userManager.CreateAsync(user);
            }

            user.LastTimeLoggedIn = DateTime.UtcNow;
            await userManager.UpdateAsync(user);

            await signInManager.SignInAsync(user, false, authProvider);
            var claims = await signInManager.CreateUserPrincipalAsync(user);
            return claims;
        }

        private static void ConfigureIdentityAuthOptions(WebApplicationBuilder builder, MicrosoftIdentityOptions options)
        {
            options.Instance = builder.Configuration.GetValue<string>("AzureAD:Instance") ?? string.Empty;
            options.TenantId = builder.Configuration.GetValue<string>("AzureAD:TenantId") ?? string.Empty;
            options.Domain = builder.Configuration.GetValue<string>("AzureAD:Domain") ?? string.Empty;
            options.ClientId = builder.Configuration.GetValue<string>("AzureAD:ClientId") ?? string.Empty;
            options.ClientSecret = builder.Configuration.GetValue<string>("AzureAD:ClientSecret") ?? string.Empty;
        }
    }
}
