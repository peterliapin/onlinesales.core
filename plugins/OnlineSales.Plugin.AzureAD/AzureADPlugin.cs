// <copyright file="AzureADPlugin.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace OnlineSales.Plugin.AzureAD;

public class JwtBearerEventsHandler : JwtBearerEvents
{
    public override async Task TokenValidated(TokenValidatedContext context)
    {
        await context.HttpContext.SignInAsync("Cookies", context.Principal!);
        await base.TokenValidated(context);
    }
}

public class AzureADPlugin : IPlugin, ISwaggerConfigurator, IPluginApplication
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        var administratorsGroupId = configuration.GetValue<string>("AzureAd:GroupsMapping:Administrators");

        services.AddAuthentication("WebApiAuthorization")
                    .AddPolicyScheme("ApiAppAuthorization", "ApiAppAuthorization", opts =>
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

                                return jwtHandler.CanReadToken(token) ? "WebApiAuthorization" : "WebAppAuthorization";
                            }

                            return "WebAppAuthorization";
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
                        }, jwtBearerScheme: "WebApiAuthorization");
        services.AddAuthentication("WebAppAuthentication")
                    .AddMicrosoftIdentityWebApp(configuration, openIdConnectScheme: "WebAppAuthorization");

        services.AddRazorPages().AddMvcOptions(options =>
        {
            var policy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
            options.Filters.Add(new AuthorizeFilter(policy));
        }).AddMicrosoftIdentityUI();

        services.AddAuthorization(options =>
        {
            options.AddPolicy("Administrators", opts =>
            {
                opts.RequireClaim(
                    "http://schemas.microsoft.com/ws/2008/06/identity/claims/role",
                    administratorsGroupId ?? Guid.NewGuid().ToString());
            });
        });
        services.Configure<CookiePolicyOptions>(options =>
        {
            options.MinimumSameSitePolicy = SameSiteMode.Strict;
            options.Secure = CookieSecurePolicy.Always;
        });
    }

    public void ConfigureSwagger(SwaggerGenOptions options, OpenApiInfo settings)
    {
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
        {
            Description = "Copy 'Bearer ' + valid JWT token into field",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer",
        });
        options.AddSecurityRequirement(new OpenApiSecurityRequirement()
        {
            {
                new OpenApiSecurityScheme()
                {
                    Reference = new OpenApiReference()
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer",
                    },
                    Scheme = "oauth2",
                    Name = "Bearer",
                    In = ParameterLocation.Header,
                },
                new List<string>()
            },
        });
    }

    public void ConfigureApplication(IApplicationBuilder application)
    {
        var app = (WebApplication)application;
        app.UseCookiePolicy();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapRazorPages();
    }
}