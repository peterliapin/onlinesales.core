// <copyright file="IdentityHelper.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using OnlineSales.Configuration;
using OnlineSales.Data;
using OnlineSales.Entities;
using OnlineSales.Identity;

namespace OnlineSales.Infrastructure;

public static class IdentityHelper
{
    // Define scheme names as constants
    public const string JwtBearerScheme = "JwtBearer";
    public const string AzureAdScheme = "AzureAd";
    public const string PolicyScheme = "Bearer";

    public static void ConfigureAuthentication(WebApplicationBuilder builder)
    {
        ConfigureIdentity(builder);

        var cookiesConfig = builder.Configuration.GetSection("Cookies").Get<CookiesConfig>();

        if (cookiesConfig != null && cookiesConfig.Enable)
        {
            ConfigureCookies(builder, cookiesConfig);
        }

        // Determine if both authentication methods are enabled
        var jwtConfig = builder.Configuration.GetSection("Jwt").Get<JwtConfig>();
        var azureAdConfig = builder.Configuration.GetSection("AzureAd").Get<AzureADConfig>();

        bool jwtEnabled = jwtConfig != null && jwtConfig.Secret != "$JWT__SECRET";
        bool azureAdEnabled = azureAdConfig != null && azureAdConfig.TenantId != "$AZUREAD__TENANTID";

        // Configure authentication with policy scheme as default
        var authBuilder = builder.Services.AddAuthentication(options =>
        {
            options.DefaultScheme = PolicyScheme;
            options.DefaultAuthenticateScheme = PolicyScheme;
            options.DefaultChallengeScheme = PolicyScheme;
        });

        // Configure the policy scheme for dynamic handler selection
        authBuilder.AddPolicyScheme(PolicyScheme, "Dynamic JWT or Azure AD", options =>
        {
            options.ForwardDefaultSelector = context =>
            {
                string? authHeader = context.Request.Headers["Authorization"];
                if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    // Extract token without the "Bearer " prefix
                    string token = authHeader.Substring("Bearer ".Length).Trim();

                    try
                    {
                        // Read the token (without validating) to inspect its issuer
                        var handler = new JsonWebTokenHandler();
                        if (handler.CanReadToken(token))
                        {
                            var jwt = handler.ReadJsonWebToken(token);
                            string issuer = jwt.Issuer;

                            // Determine local issuer
                            string? localIssuer = jwtEnabled && jwtConfig != null ? jwtConfig.Issuer : string.Empty;

                            // Select scheme based on issuer
                            if (!string.IsNullOrEmpty(issuer))
                            {
                                if (azureAdEnabled && azureAdConfig != null && issuer.StartsWith(azureAdConfig.Instance))
                                {
                                    return AzureAdScheme;
                                }

                                if (jwtEnabled && issuer == localIssuer)
                                {
                                    return JwtBearerScheme;
                                }
                            }
                        }
                    }
                    catch
                    {
                        // Token read failed, continue to default
                    }
                }

                // Default scheme selection based on what's enabled
                if (jwtEnabled)
                {
                    return JwtBearerScheme;
                }
                else if (azureAdEnabled)
                {
                    return AzureAdScheme;
                }

                // Fallback to JWT if nothing specific is determined
                return JwtBearerScheme;
            };
        });

        if (jwtEnabled)
        {
            ConfigureInternalJwt(authBuilder, jwtConfig!, JwtBearerScheme);
        }

        if (azureAdEnabled)
        {
            ConfigureAzureAd(builder, authBuilder, azureAdConfig!, AzureAdScheme);
        }
    }

    public static void ConfigureAuthorization(WebApplicationBuilder builder)
    {
        builder.Services.AddAuthorization(options =>
        {
            // Default policy that requires authenticated user from either JwtBearer or AzureAd scheme
            options.DefaultPolicy = new AuthorizationPolicyBuilder()
                .AddAuthenticationSchemes(JwtBearerScheme, AzureAdScheme)
                .RequireAuthenticatedUser()
                .Build();
        });
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

    public static void ConfigureInternalJwt(AuthenticationBuilder authBuilder, JwtConfig jwtConfig, string schemeName)
    {
        authBuilder.AddJwtBearer(schemeName, options =>
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

    public static void ConfigureAzureAd(WebApplicationBuilder builder, AuthenticationBuilder authBuilder, AzureADConfig azureAdConfig, string schemeName)
    {
        // Configure JWT Bearer for API validation
        authBuilder.AddMicrosoftIdentityWebApi(
            jwtOptions =>
            {
                jwtOptions.Events = new AzureAdJwtBearerEventsHandler(builder.Configuration);

                // Add the following to handle CORS preflight requests
                jwtOptions.Events.OnChallenge = async context =>
                {
                    // Skip Microsoft Identity challenges for OPTIONS requests (CORS preflight)
                    if (context.Request.Method == "OPTIONS")
                    {
                        context.HandleResponse();
                        return;
                    }

                    await Task.CompletedTask;
                };
            },
            identityOptions =>
            {
                ConfigureIdentityAuthOptions(azureAdConfig, identityOptions);
            },
            schemeName);

        // Add OpenID Connect for interactive login - no need to register cookie handler separately
        // as AddMicrosoftIdentityWebApp will register it automatically
        authBuilder.AddMicrosoftIdentityWebApp(
            identityOptions =>
            {
                ConfigureIdentityAuthOptions(azureAdConfig, identityOptions);
                identityOptions.CallbackPath = "/api/identity/azure-login-callback";
                identityOptions.SignedOutCallbackPath = "/api/identity/signout-callback-oidc";
                identityOptions.SkipUnrecognizedRequests = true;

                // Ensure correct redirect behavior
                identityOptions.ResetPasswordPath = "/api/identity/reset-password";
                identityOptions.ErrorPath = "/api/identity/error";
            },
            cookieOptions =>
            {
                cookieOptions.Cookie.Name = "AzureAdAuth_ticket";
                cookieOptions.Events = new AzureAdCookieEventsHandler();

                // Make sure cookie can be shared with the frontend
                cookieOptions.Cookie.SameSite = SameSiteMode.None;
                cookieOptions.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                cookieOptions.ExpireTimeSpan = TimeSpan.FromHours(12);
            },
            "AzureAdOpenID",
            "AzureAdCookies");
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