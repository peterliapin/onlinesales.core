// <copyright file="IdentityHelper.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Tokens;
using OnlineSales.Configuration;
using OnlineSales.Data;
using OnlineSales.Entities;
using OnlineSales.Identity;

namespace OnlineSales.Infrastructure
{
    public static class IdentityHelper
    {
        public static void ConfigureIdentity(WebApplicationBuilder builder)
        {
            var jwtConfig = builder.Configuration.GetSection("Jwt").Get<JwtConfig>();
            var azureAdConfig = builder.Configuration.GetSection("AzureAd").Get<AzureADConfig>();
            var lockoutConfig = builder.Configuration.GetSection("DefaultUserLockout").Get<DefaultUserLockoutConfig>();

            builder.Services.AddIdentity<User, IdentityRole>(options =>
            {
                // Lockout settings
                if (lockoutConfig != null)
                {
                    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(lockoutConfig.LockoutTime);
                    options.Lockout.MaxFailedAccessAttempts = lockoutConfig.MaxFailedAccessAttempts;
                    options.Lockout.AllowedForNewUsers = true;
                }
            })
                .AddEntityFrameworkStores<PgDbContext>()
                .AddDefaultTokenProviders();

            builder.Services.ConfigureApplicationCookie(options =>
                {
                    // Cookie settings
                    options.Cookie.HttpOnly = true;
                    options.ExpireTimeSpan = TimeSpan.FromHours(12);
                    options.Cookie.Name = "auth_ticket";

                    options.LoginPath = "/api/identity/external-login";
                    options.AccessDeniedPath = "/access-denied";
                    options.SlidingExpiration = true;
                });

            var authBuilder = builder.Services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                });

            if (jwtConfig != null && jwtConfig.Secret != "$JWT__SECRET")
            {
                authBuilder.AddJwtBearer(options =>
                {
                    options.SaveToken = true;
                    options.RequireHttpsMetadata = false;
                    options.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidAudience = jwtConfig.Audience,
                        ValidIssuer = jwtConfig.Issuer,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig.Secret)),
                    };
                });
            }

            if (azureAdConfig != null && azureAdConfig.TenantId != "$AZUREAD__TENANTID")
            {
                authBuilder.AddMicrosoftIdentityWebApi(
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
            }

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