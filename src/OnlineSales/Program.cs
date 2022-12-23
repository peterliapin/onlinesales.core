// <copyright file="Program.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Net;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using OnlineSales.Configuration;
using OnlineSales.Controllers;
using OnlineSales.Data;
using OnlineSales.ErrorHandling;
using OnlineSales.Infrastructure;
using OnlineSales.Interfaces;
using OnlineSales.Services;
using OnlineSales.Tasks;
using Quartz;
using Serilog.Exceptions;
using Serilog.Sinks.Elasticsearch;
using Swashbuckle.AspNetCore;

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

        AppSettingsFiles.ForEach(path =>
        {
            builder.Configuration.AddJsonFile(path, false, true);
        });

        ConfigureLogs(builder);
        PluginManager.Init(builder.Configuration);

        builder.Configuration.AddUserSecrets(typeof(Program).Assembly);
        builder.Configuration.AddEnvironmentVariables();

        builder.Services.AddSingleton<IErrorMessageGenerator, ErrorMessageGenerator>();
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddSingleton<IHttpContextHelper, HttpContextHelper>();
        builder.Services.AddTransient<IOrderItemService, OrderItemService>();
        builder.Services.AddScoped<IVariablesService, VariablesService>();

        ConfigureCacheProfiles(builder);

        ConfigureConventions(builder);
        ConfigureControllers(builder);
        ConfigurePostgres(builder);
        ConfigureElasticsearch(builder);
        ConfigureQuartz(builder);
        ConfigureImageUpload(builder);
        ConfigureEmailServices(builder);
        ConfigureTasks(builder);

        builder.Services.AddAutoMapper(typeof(Program));
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddControllers()
            .ConfigureApiBehaviorOptions(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });
        ConfigureSwagger(builder);

        builder.Services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.All;
        });

        ConfigureCORS(builder);

        app = builder.Build();

        app.UseMiddleware<ErrorMessageMiddleware>();

        app.UseForwardedHeaders();

        MigrateOnStartIfRequired(app, builder);

        // Configure the HTTP request pipeline.
        // if (app.Environment.IsDevelopment())
        // {
        // app.UseODataRouteDebug();
        // }

        app.UseSwagger();
        app.UseSwaggerUI();

        app.UseHttpsRedirection();
        app.UseDefaultFiles();
        app.UseStaticFiles();
        app.UseCors();

        PluginManager.Init(app);

        app.MapControllers();

        app.UseSpa(spa =>
        {
            // works out of the box, no configuration required
        });

        app.Run();
    }

    private static void ConfigureLogs(WebApplicationBuilder builder)
    {
        var elasticConfig = builder.Configuration.GetSection("ElasticSearch").Get<ElasticsearchConfig>();

        if (elasticConfig == null)
        {
            throw new MissingConfigurationException("Elasticsearch configuraiton is mandatory.");
        }

        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .Enrich.WithExceptionDetails()
            .WriteTo.Console()
            .WriteTo.Elasticsearch(ConfigureELK(elasticConfig))
            .CreateLogger();

        builder.Host.UseSerilog();
    }

    private static ElasticsearchSinkOptions ConfigureELK(ElasticsearchConfig elasticConfig)
    {
        var uri = new Uri(elasticConfig.Url);

        return new ElasticsearchSinkOptions(uri)
        {
            AutoRegisterTemplate = true,
            AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv7,
            IndexFormat = $"{elasticConfig.IndexPrefix}-logs",
        };
    }

    private static void MigrateOnStartIfRequired(WebApplication app, WebApplicationBuilder builder)
    {
        var migrateOnStart = builder.Configuration.GetValue<bool>("MigrateOnStart");

        if (migrateOnStart)
        {
            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
                context.Database.Migrate();

                var pluginContexts = scope.ServiceProvider.GetServices<PluginDbContextBase>();

                foreach (var pluginContext in pluginContexts)
                {
                    pluginContext.Database.Migrate();
                }
            }
        }
    }

    private static void ConfigureConventions(WebApplicationBuilder builder)
    {
        builder.Services.Configure<RouteOptions>(options =>
        {
            options.LowercaseUrls = true;
            options.LowercaseQueryStrings = true;
        });

        builder.Services.AddControllers(options => options.Conventions.Add(new RouteTokenTransformerConvention(new RouteToKebabCase())));
    }

    private static void ConfigureControllers(WebApplicationBuilder builder)
    {
        var controllersBuilder = builder.Services.AddControllers()
            .AddJsonOptions(opts =>
            {
                var enumConverter = new JsonStringEnumConverter();
                opts.JsonSerializerOptions.Converters.Add(enumConverter);
                opts.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            })
            /*.AddOData(options => options
                .Select().Filter().OrderBy()
                .SetMaxTop(10).Expand().Count()
                .SkipToken())*/;

        foreach (var plugin in PluginManager.GetPluginList())
        {
            controllersBuilder = controllersBuilder.AddApplicationPart(plugin.GetType().Assembly).AddControllersAsServices();
            plugin.ConfigureServices(builder.Services, builder.Configuration);
        }
    }

    private static void ConfigurePostgres(WebApplicationBuilder builder)
    {
        builder.Services.AddDbContext<ApiDbContext>();
    }

    private static void ConfigureElasticsearch(WebApplicationBuilder builder)
    {
        var elasticConfig = builder.Configuration.GetSection("ElasticSearch").Get<ElasticsearchConfig>();

        if (elasticConfig == null)
        {
            throw new MissingConfigurationException("Elasticsearch configuraiton is mandatory.");
        }

        builder.Services.AddElasticsearch(elasticConfig);
    }

    private static void ConfigureImageUpload(WebApplicationBuilder builder)
    {
        var imageUploadConfig = builder.Configuration.GetSection("Images");

        if (imageUploadConfig == null)
        {
            throw new MissingConfigurationException("Image Upload configuraiton is mandatory.");
        }

        builder.Services.Configure<ImagesConfig>(imageUploadConfig);
    }

    private static void ConfigureSwagger(WebApplicationBuilder builder)
    {
        var openApiInfo = new OpenApiInfo()
        {
            Version = typeof(Program).Assembly.GetName().Version!.ToString() ?? "1.0.0",
            Title = "OnlineSales API",
        };
        var swaggerConfigurators = from p in PluginManager.GetPluginList()
                                   where p is ISwaggerConfigurator
                                   select p as ISwaggerConfigurator;

        builder.Services.AddSwaggerGen(config =>
        {
            foreach (var swaggerConfigurator in swaggerConfigurators)
            {
                swaggerConfigurator.ConfigureSwagger(config, openApiInfo);
            }

            config.SwaggerDoc("v1", openApiInfo);
        });
    }

    private static void ConfigureQuartz(WebApplicationBuilder builder)
    {
        builder.Services.AddQuartz(q =>
        {
            q.UseMicrosoftDependencyInjectionJobFactory();

            q.AddJob<TaskRunner>(opts => opts.WithIdentity("TaskRunner"));

            q.AddTrigger(opts =>
                opts.ForJob("TaskRunner").WithIdentity("TaskRunner").WithCronSchedule(builder.Configuration.GetValue<string>("TaskRunner:CronSchedule") !));
        });

        builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);
    }

    private static void ConfigureCacheProfiles(WebApplicationBuilder builder)
    {
        var cacheProfiles = builder.Configuration.GetSection("CacheProfiles").Get<List<CacheProfileSettings>>();

        if (cacheProfiles == null)
        {
            throw new MissingConfigurationException("Image Upload configuraiton is mandatory.");
        }

        builder.Services.AddControllers(options =>
        {
            foreach (var item in cacheProfiles)
            {
                options.CacheProfiles.Add(
                    item!.Type!,
                    new CacheProfile()
                    {
                        Duration = item!.Duration,
                        VaryByHeader = item!.VaryByHeader!,
                    });
            }
        });
    }

    private static void ConfigureEmailServices(WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IEmailWithLogService, EmailWithLogService>();

        builder.Services.AddScoped<IEmailFromTemplateService, EmailFromTemplateService>();
    }

    private static void ConfigureTasks(WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<ITask, CustomerScheduledEmail>();
    }

    private static void ConfigureCORS(WebApplicationBuilder builder)
    {
        var corsSettings = builder.Configuration.GetSection("Cors").Get<CorsConfig>();
        if (corsSettings == null)
        {
            throw new MissingConfigurationException("CORS configuraiton is mandatory.");
        }

        if (!corsSettings.AllowedOrigins.Any())
        {
            throw new MissingConfigurationException("Specify CORS allowed domains (Use '*' to allow any ).");
        }

        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy
                    .AllowAnyMethod()
                    .AllowAnyHeader();
                if (corsSettings.AllowedOrigins.FirstOrDefault() == "*")
                {
                    policy.AllowAnyOrigin();
                }
                else
                {
                    policy.WithOrigins(corsSettings.AllowedOrigins);
                }
            });
        });
    }
}