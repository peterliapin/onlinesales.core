// <copyright file="MicrosoftAccountPlugin.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OnlineSales.Interfaces;

namespace OnlineSales.Plugin.MicrosoftAccount;

public class MicrosoftAccountPlugin : IPlugin
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        var config = configuration.GetSection("MicrosoftAccount");
        services.AddAuthentication().AddMicrosoftAccount(options =>
        {
            options.ClientId = config.GetValue<string>("ClientId") ?? throw new Exception();
            options.ClientSecret = config.GetValue<string>("ClientSecret") ?? throw new Exception();
            options.AuthorizationEndpoint = config.GetValue<string>("AuthorizationEndpoint") ?? throw new Exception();
            options.TokenEndpoint = config.GetValue<string>("TokenEndpoint") ?? throw new Exception();
            options.CorrelationCookie.SameSite = SameSiteMode.Unspecified;
            options.Scope.Add("api://1f6244ca-1644-42de-ad06-c41bc8286bcb/API_Scope");
        });
    }
}