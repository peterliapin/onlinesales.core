// <copyright file="Program.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Reflection;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using OnlineSales.Configuration;
using OnlineSales.Data;
using Serilog;
using Serilog.Exceptions;
using Serilog.Sinks.Elasticsearch;

namespace OnlineSales;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var servicesConfig = builder.Configuration.Get<AppSettings>();

        ConfigureLogs(servicesConfig.ElasticSearch.Url);

        builder.Host.UseSerilog();

        ConfigureConventions(builder);

        builder.Services.AddAutoMapper(typeof(Program));

        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddSwaggerGen();

        ConfigurePostgres(builder, servicesConfig.Postgres);

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }

    private static void ConfigureLogs(string elasticSearchUrl)
    {
        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .Enrich.WithExceptionDetails()
            .WriteTo.Debug()
            .WriteTo.Console()
            .WriteTo.Elasticsearch(ConfigureELK(elasticSearchUrl))
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

        builder.Services
            .AddMvc()
            .AddJsonOptions(opts =>
            {
                var enumConverter = new JsonStringEnumConverter();
                opts.JsonSerializerOptions.Converters.Add(enumConverter);
            });

        builder.Services.AddControllers().AddJsonOptions(
            options => options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull);
    }

    private static void ConfigurePostgres(WebApplicationBuilder builder, PostgresConfig config)
    {
        builder.Services.AddEntityFrameworkNpgsql()
            .AddDbContext<ApiDbContext>(
                opt => opt
                .UseNpgsql(
                    config.ConnectionString,
                    b => b.MigrationsHistoryTable("_migrations"))
                .UseSnakeCaseNamingConvention());
    }
}