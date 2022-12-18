// <copyright file="TestApplication.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OnlineSales.Data;
using OnlineSales.Interfaces;
using OnlineSales.Tests.TestServices;

namespace OnlineSales.Tests.Environment;

public class TestApplication : WebApplicationFactory<Program>
{
    public TestApplication()
    {
        var projectDir = Directory.GetCurrentDirectory();
        var configPath = Path.Combine(projectDir, "appsettings.tests.json");

        Program.AddAppSettingsJsonFile(configPath);
    }

    public void CleanDatabase()
    {
        using (var scope = Services.CreateScope())
        {
            var dataContaxt = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
            dataContaxt.Database.EnsureDeleted();
            dataContaxt.Database.Migrate();
        }
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.AddScoped<IEmailService, TestEmailService>();

            services.AddAuthentication(defaultScheme: "TestScheme")
                .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>(
                    "TestScheme", options => { });
        });

        return base.CreateHost(builder);
    }
}

