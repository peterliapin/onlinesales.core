// <copyright file="IdentityHelper.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Tokens;
using OnlineSales.Configuration;
using OnlineSales.Data;
using OnlineSales.Entities;
using OnlineSales.Identity;

namespace OnlineSales.Infrastructure;

public static class IdentityHelper
{
    public const string AzureAdJwtAuthScheeme = "AzureAdBearer";

    public static void ConfigureAuthentication(WebApplicationBuilder builder)
    {
        ConfigureIdentity(builder);

        var cookiesConfig = builder.Configuration.GetSection("Cookies").Get<CookiesConfig>();

        if (cookiesConfig != null && cookiesConfig.Enable)
        {
            ConfigureCookies(builder, cookiesConfig);
        }

        var authBuilder = builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        });

        var jwtConfig = builder.Configuration.GetSection("Jwt").Get<JwtConfig>();

        if (jwtConfig != null && jwtConfig.Secret != "$JWT__SECRET")
        {
            ConfigureInternalJwt(authBuilder, jwtConfig);
        }

        var azureAdConfig = builder.Configuration.GetSection("AzureAd").Get<AzureADConfig>();

        if (azureAdConfig != null && azureAdConfig.TenantId != "$AZUREAD__TENANTID")
        {
            ConfigureAzureAd(builder, authBuilder, azureAdConfig);
        }
    }

    public static void ConfigureCookies(WebApplicationBuilder builder, CookiesConfig cookiesConfig)
    {
        builder.Services.ConfigureApplicationCookie(options =>
        {
            // Cookie settings
            options.Cookie.HttpOnly = true;
            options.ExpireTimeSpan = TimeSpan.FromHours(cookiesConfig.ExpireTime);
            options.Cookie.Name = cookiesConfig.Name;

            options.LoginPath = "/api/identity/external-login";
            options.AccessDeniedPath = "/access-denied";
            options.SlidingExpiration = true;
        });

        builder.Services.Configure<CookiePolicyOptions>(options =>
        {
            options.MinimumSameSitePolicy = SameSiteMode.None;
            options.Secure = CookieSecurePolicy.Always;
        });
    }

    public static void ConfigureIdentity(WebApplicationBuilder builder)
    {
        var identityConfig = builder.Configuration.GetSection("Identity").Get<IdentityConfig>();

        builder.Services.AddIdentity<User, IdentityRole>(options =>
        {
            // Lockout settings
            if (identityConfig != null)
            {
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(identityConfig.LockoutTime);
                options.Lockout.MaxFailedAccessAttempts = identityConfig.MaxFailedAccessAttempts;
                options.Lockout.AllowedForNewUsers = true;
            }
        })
        .AddEntityFrameworkStores<PgDbContext>()
        .AddDefaultTokenProviders();
    }

    public static void ConfigureInternalJwt(AuthenticationBuilder authBuilder, JwtConfig jwtConfig)
    {
        authBuilder.AddJwtBearer(options =>
        {
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

    public static void ConfigureAzureAd(WebApplicationBuilder builder, AuthenticationBuilder authBuilder, AzureADConfig azureAdConfig)
    {
        authBuilder.AddMicrosoftIdentityWebApi(
            jwtOptions =>
            {
                jwtOptions.Events = new AzureAdJwtBearerEventsHandler();
            }, identityOptions =>
            {
                ConfigureIdentityAuthOptions(azureAdConfig, identityOptions);
            }, AzureAdJwtAuthScheeme);

        builder.Services.AddAuthentication().AddMicrosoftIdentityWebApp(
            identityOptions =>
            {
                ConfigureIdentityAuthOptions(azureAdConfig, identityOptions);

                identityOptions.CallbackPath = "/api/identity/callback";
                identityOptions.SkipUnrecognizedRequests = true;
            }, cookieOptions =>
            {
                cookieOptions.Cookie.Name = "auth_ticket";
                cookieOptions.Events = new AzureAdCookieEventsHandler();
            });
    }

    private static void ConfigureIdentityAuthOptions(AzureADConfig azureAdConfig, MicrosoftIdentityOptions options)
    {
        options.Instance = azureAdConfig.Instance;
        options.TenantId = azureAdConfig.TenantId;
        options.Domain = azureAdConfig.Domain;
        options.ClientId = azureAdConfig.ClientId;
        options.ClientSecret = azureAdConfig.ClientSecret;
    }
}