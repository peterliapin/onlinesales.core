// <copyright file="Program.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Reflection;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using OnlineSales.Backend.Infrastructure;
using OnlineSales.Configuration;
using OnlineSales.Data;
using Serilog;
using Serilog.Exceptions;
using Serilog.Sinks.Elasticsearch;

namespace OnlineSales;

public class Program
{
    private static readonly List<string> AppSettingsFiles = new List<string>();

    private static WebApplication? app;

    public static WebApplication? GetApp()
    {
        return app;
    }

    public static void AddAppSettingsJsonFile(string path)
    {
        AppSettingsFiles.Add(path);
    }

    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        ConfigureLogs(builder);

        PluginManager.Init();

        AppSettingsFiles.ForEach(path =>
        {
            builder.Configuration.AddJsonFile(path, false, true);
        });

        ConfigureConventions(builder);

        ConfigureControllers(builder);

        builder.Services.AddAutoMapper(typeof(Program));

        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddSwaggerGen();

        ConfigurePostgres(builder);

        builder.Host.UseSerilog();

        app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseDefaultFiles();
        app.UseStaticFiles();
        app.UseAuthorization();
        app.MapControllers();

        app.Run();
    }

    private static void ConfigureLogs(WebApplicationBuilder builder)
    {
        var elasticConfig = builder.Configuration.GetSection("ElasticSearch").Get<ElasticSearchConfig>();

        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .Enrich.WithExceptionDetails()
            .WriteTo.Debug()
            .WriteTo.Console()
            .WriteTo.Elasticsearch(ConfigureELK(elasticConfig.Url))
            .CreateLogger();
    }

    private static ElasticsearchSinkOptions ConfigureELK(string elasticSearchUrl)
    {      
        var uri = new Uri(elasticSearchUrl);

        var assemblyName = Assembly.GetExecutingAssembly().GetName()
            !.Name!.ToLower();

        return new ElasticsearchSinkOptions(uri)
        {
            AutoRegisterTemplate = true,
            AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv7,
            IndexFormat = $"{assemblyName}-logs",
        };
    }

    private static void ConfigureConventions(WebApplicationBuilder builder)
    {
        builder.Services.Configure<RouteOptions>(options =>
        {
            options.LowercaseUrls = true;
            options.LowercaseQueryStrings = true;
        });
    }

    private static void ConfigureControllers(WebApplicationBuilder builder)
    {
        var controllersBuilder = builder.Services.AddControllers()
            .AddJsonOptions(opts =>
            {
                var enumConverter = new JsonStringEnumConverter();
                opts.JsonSerializerOptions.Converters.Add(enumConverter);
                opts.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            });
        foreach (var plugin in PluginManager.GetPluginList())
        {
            controllersBuilder = controllersBuilder.AddApplicationPart(plugin.GetType().Assembly).AddControllersAsServices();
            plugin.ConfigureServices(builder.Services, builder.Configuration);
        }
    }

    private static void ConfigurePostgres(WebApplicationBuilder builder)
    {
        builder.Services.AddEntityFrameworkNpgsql()
            .AddDbContext<ApiDbContext>(
                opt =>
                {
                    var postgresConfig = builder.Configuration.GetSection("Postgres").Get<PostgresConfig>();

                    opt.UseNpgsql(
                        postgresConfig.ConnectionString,
                        b => b.MigrationsHistoryTable("_migrations"))
                    .UseSnakeCaseNamingConvention();
                });
    }
}