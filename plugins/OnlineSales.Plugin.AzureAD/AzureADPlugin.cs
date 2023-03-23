// <copyright file="AzureADPlugin.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using OnlineSales.Plugin.AzureAD.Exceptions;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace OnlineSales.Plugin.AzureAD;

public class AzureADPlugin : IPlugin, ISwaggerConfigurator, IPluginApplication
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        var administratorsGroupId = configuration.GetValue<string>("AzureAd:GroupsMapping:Administrators");

        services.ConfigureAuth(configuration);

        services.AddAuthorization(options =>
        {
            options.AddPolicy("Administrators", opts =>
            {
                if (string.IsNullOrEmpty(administratorsGroupId) || !Guid.TryParse(administratorsGroupId, out _))
                {
                    throw new MissingAdminGroupException("AzureAd:GroupsMapping:Administrators field is required.");
                }

                opts.RequireClaim(
                    "http://schemas.microsoft.com/ws/2008/06/identity/claims/role",
                    administratorsGroupId);
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