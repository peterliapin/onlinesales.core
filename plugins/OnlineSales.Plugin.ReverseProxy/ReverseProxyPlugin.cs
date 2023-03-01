// <copyright file="ReverseProxyPlugin.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OnlineSales.Exceptions;
using OnlineSales.Interfaces;

namespace OnlineSales.Plugin.ReverseProxy;

public class ReverseProxyPlugin : IPlugin, IPluginApplication
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        var proxyConfig = configuration.GetSection("YARPSettings");
        if (proxyConfig == null)
        {
            throw new MissingConfigurationException("YARP configuration is mandatory.");
        }

        services.AddAuthorization(options =>
        {
            options.AddPolicy("ProxyAuth", policy =>
                policy.RequireAuthenticatedUser().AddAuthenticationSchemes("EntryAuthorization")); // First try to authenticate with bearer ( if provided ). Then with OpenID. Required for <iframe> integration.
        });

        services.AddReverseProxy().LoadFromConfig(proxyConfig);
    }

    public void ConfigureApplication(IApplicationBuilder application)
    {
        var app = (WebApplication)application;
        app.MapReverseProxy();
    }
}