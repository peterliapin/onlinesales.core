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
        await context.HttpContext.SignInAsync("Cookies", context.Principal !);
        await base.TokenValidated(context);
    }

    public override async Task Forbidden(ForbiddenContext context)
    {
        await context.HttpContext.ChallengeAsync("WebAppAuthorization");
    }

#pragma warning disable
    public override async Task MessageReceived(MessageReceivedContext context)
    {
        await base.MessageReceived(context);
    }

    public override async Task AuthenticationFailed(AuthenticationFailedContext context)
    {
        await base.AuthenticationFailed(context);
    }
    public override async Task Challenge(JwtBearerChallengeContext context)
    {
        // await context.HttpContext.ChallengeAsync("WebAppAuthorization");
        // context.HandleResponse();
        base.Challenge(context);
    }
#pragma warning restore
}

public class AzureADPlugin : IPlugin, ISwaggerConfigurator, IPluginApplication
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        var administratorsGroupId = configuration.GetValue<string>("AzureAd:GroupsMapping:Administrators");

        services.AddAuthentication("WebApiAuthorization")
                    // .AddMicrosoftIdentityWebApi(configuration, subscribeToJwtBearerMiddlewareDiagnosticsEvents: true, jwtBearerScheme: "WebApiAuthorization");
                    .AddPolicyScheme("EntryAuthorization", "EntryAuthorization", opts =>
                    {
                        opts.ForwardDefaultSelector = ctx =>
                        {
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
                        identityOptions.Instance = "https://login.microsoftonline.com/";
                        identityOptions.TenantId = "f1426473-3abb-49a9-8b8c-a7fe9420e5dd";
                        identityOptions.Domain = "waveaccess.global";
                        identityOptions.ClientId = "1f6244ca-1644-42de-ad06-c41bc8286bcb";
                        identityOptions.ClientSecret = "mlo8Q~8XO1YdQuTT8LfPjksfUJni.DHdFulOraAm";
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
            options.MinimumSameSitePolicy = SameSiteMode.None;
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