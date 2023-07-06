// <copyright file="TestApplication.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Linq.Expressions;
using AutoMapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Nest;
using OnlineSales.Data;
using OnlineSales.Plugin.TestPlugin.Data;
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
            RenewDatabase(dataContaxt);

            var esDbContext = scope.ServiceProvider.GetRequiredService<EsDbContext>();
            esDbContext.ElasticClient.Indices.Delete("*");
        }
    }

    public ElasticClient GetElasticClient()
    {
        using (var scope = Services.CreateScope())
        {
            var esDbContext = scope.ServiceProvider.GetRequiredService<EsDbContext>();
            return esDbContext.ElasticClient;
        }
    }

    public void PopulateBulkData<T, TS>(dynamic bulkItems)
        where T : BaseEntityWithId
        where TS : ISaveService<T>
    {
        using (var scope = Services.CreateScope())
        {
            var dataContaxt = scope.ServiceProvider.GetRequiredService<PgDbContext>();

            var saveService = scope.ServiceProvider.GetService<TS>();

            if (saveService != null)
            {
                saveService.SaveRangeAsync(bulkItems).Wait();
            }
            else
            {
                dataContaxt.AddRange(bulkItems);
            }

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
            return serviceScope.ServiceProvider.GetService<IMapper>()!;
        }
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.AddScoped<TestPluginDbContext, TestPluginDbContext>();

            services.AddScoped<IEmailService, TestEmailService>();
            services.AddScoped<IEmailValidationExternalService, TestEmailValidationExternalService>();
            services.AddScoped<IAccountExternalService, TestAccountExternalService>();

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = TestAuthenticationHandler.SchemeName;
                options.DefaultChallengeScheme = TestAuthenticationHandler.SchemeName;
            })
                .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>(
                    TestAuthenticationHandler.SchemeName, options => { });
        });

        return base.CreateHost(builder);
    }

    private void RenewDatabase(PgDbContext context)
    {
        try
        {
            context.Database.EnsureDeleted();
            context.Database.Migrate();
        }
        catch
        {
            Thread.Sleep(1000);
            RenewDatabase(context);
        }
    }
}