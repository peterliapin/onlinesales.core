// <copyright file="TestApplication.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using AutoMapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Nest;
using OnlineSales.Data;
using OnlineSales.Interfaces;
using OnlineSales.Tasks;
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
            var dataContaxt = scope.ServiceProvider.GetRequiredService<PgDbContext>();
            dataContaxt.Database.EnsureDeleted();
            dataContaxt.Database.Migrate();

            var esDbContext = scope.ServiceProvider.GetRequiredService<EsDbContext>();
            esDbContext.ElasticClient.Indices.Delete("*");
        }
    }

    public void PopulateBulkData(dynamic bulkItems)
    {
        using (var scope = Services.CreateScope())
        {
            var dataContaxt = scope.ServiceProvider.GetRequiredService<PgDbContext>();
            dataContaxt.AddRange(bulkItems);
            dataContaxt.SaveChangesAsync().Wait();
        }
    }

    public PgDbContext? GetDbContext()
    {
        var scope = Services.CreateScope();
        var dataContext = scope.ServiceProvider.GetRequiredService<PgDbContext>();
        return dataContext;
    }

    public IMapper GetMapper()
    {
        using (var serviceScope = Services.CreateScope())
        {
            return serviceScope.ServiceProvider.GetService<IMapper>() !;
        }
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.AddScoped<IEmailService, TestEmailService>();
            services.AddScoped<IEmailValidationExternalService, TestEmailValidationExternalService>();
            services.AddScoped<IAccountExternalService, TestAccountExternalService>();

            services.AddAuthentication(defaultScheme: "TestScheme")
                .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>(
                    "TestScheme", options => { });
        });

        return base.CreateHost(builder);
    }
}