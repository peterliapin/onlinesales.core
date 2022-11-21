// <copyright file="AzureADPlugin.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Web;
using NSwag;
using NSwag.Generation.AspNetCore;
using NSwag.Generation.Processors.Security;
using OnlineSales.Configuration;
using Serilog;

namespace OnlineSales.Plugin.AzureAD;

public class AzureADPlugin : IPlugin, ISwaggerConfigurator
{
    public void Configure(IServiceCollection services, IConfiguration configuration)
    {
        var administratorsGroupId = configuration.GetValue<string>("AzureAd:GroupsMapping:Administrators");

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddMicrosoftIdentityWebApi(configuration);

        services.AddAuthorization(options =>
        {
            options.AddPolicy("Administrators", opts =>
            {
                opts.RequireClaim(
                    "http://schemas.microsoft.com/ws/2008/06/identity/claims/role",
                    administratorsGroupId ?? Guid.NewGuid().ToString());
            });
        });
    }

    public void ConfigureSwagger(AspNetCoreOpenApiDocumentGeneratorSettings settings)
    {
        settings.OperationProcessors.Add(new OperationSecurityScopeProcessor("Azure AD JWT Token"));
        settings.AddSecurity("Azure JWT Token", new OpenApiSecurityScheme
        {
            Type = OpenApiSecuritySchemeType.ApiKey,
            Name = "Authorization",
            Description = "Copy 'Bearer ' + valid JWT token into field",
            In = OpenApiSecurityApiKeyLocation.Header,
        });
    }
}