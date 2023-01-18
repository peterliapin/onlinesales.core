// <copyright file="Program.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using OnlineSales.Configuration;
using OnlineSales.Data;
using OnlineSales.Formatters.Csv;
using OnlineSales.Helpers;
using OnlineSales.Infrastructure;
using OnlineSales.Interfaces;
using OnlineSales.Services;
using OnlineSales.Tasks;
using Quartz;
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

        AppSettingsFiles.ForEach(path =>
        {
            builder.Configuration.AddJsonFile(path, false, true);
        });

        ConfigureLogs(builder);
        PluginManager.Init(builder.Configuration);

        builder.Configuration.AddUserSecrets(typeof(Program).Assembly);
        builder.Configuration.AddEnvironmentVariables();

        builder.Services.AddHttpContextAccessor();
        builder.Services.AddSingleton<IHttpContextHelper, HttpContextHelper>();
        builder.Services.AddTransient<IDomainCheckService, DomainCheckService>();
        builder.Services.AddTransient<IOrderItemService, OrderItemService>();
        builder.Services.AddScoped<IVariablesService, VariablesService>();
        builder.Services.AddSingleton<IpDetailsService, IpDetailsService>();
        builder.Services.AddSingleton<ILockService, LockService>();

        ConfigureCacheProfiles(builder);

        ConfigureConventions(builder);
        ConfigureControllers(builder);
        ConfigurePostgres(builder);
        ConfigureElasticSearch(builder);
        ConfigureQuartz(builder);
        ConfigureImageUpload(builder);
        ConfigureIpDetailsResolver(builder);
        ConfigureEmailServices(builder);
        ConfigureTasks(builder);
        ConfigureApiSettings(builder);
        ConfigureImportSizeLimit(builder);

        builder.Services.AddAutoMapper(typeof(Program));
        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddControllers(options =>
        {
            options.RespectBrowserAcceptHeader = true;
            options.ReturnHttpNotAcceptable = true;
            options.OutputFormatters.RemoveType<StringOutputFormatter>();
            options.InputFormatters.Add(new CsvInputFormatter());
            options.OutputFormatters.Add(new CsvOutputFormatter());
            options.FormatterMappings.SetMediaTypeMappingForFormat("csv", "text/csv");
        })
        .AddXmlSerializerFormatters()
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

        app.UseHttpsRedirection();
        app.UseExceptionHandler("/error");
        app.UseForwardedHeaders();

        MigrateOnStartIfRequired(app, builder);

        app.UseSwagger();
        app.UseSwaggerUI();
        app.UseDefaultFiles();
        app.UseStaticFiles();
        app.UseCors();

        PluginManager.Init(app);

        app.MapControllers();

        SetImageUploadSizeLimit(app, builder);

        app.UseSpa(spa =>
        {
            // works out of the box, no configuration required
        });

        app.Run();
    }

    private static void SetImageUploadSizeLimit(WebApplication app, WebApplicationBuilder builder)
    {
        var maxUploadSizeConfig = builder.Configuration.GetValue<string>("Images:MaxSize");

        if (string.IsNullOrEmpty(maxUploadSizeConfig))
        {
            throw new MissingConfigurationException("Image upload size is mandatory.");
        }

        long? maxUploadSize = StringHelper.GetSizeInBytesFromString(maxUploadSizeConfig);

        if (maxUploadSize is null)
        {
            throw new MissingConfigurationException("Image upload size is invalid.");
        }

        app.UseWhen(
            context => context.Request.Method == "POST" && context.Request.Path.StartsWithSegments("/api/images"),
            appBuilder => appBuilder.Use(async (c, next) =>
            {
                var feature = c.Features.Get<IHttpMaxRequestBodySizeFeature>();
                if (feature is not null)
                {
                    feature.MaxRequestBodySize = maxUploadSize; 
                }

                await next();
            }));
    }

    private static void ConfigureImportSizeLimit(WebApplicationBuilder builder)
    {
        var maxImportSizeConfig = builder.Configuration.GetValue<string>("ApiSettings:MaxImportSize");

        if (string.IsNullOrEmpty(maxImportSizeConfig))
        {
            throw new MissingConfigurationException("Import file size is mandatory.");
        }

        var maxImportSize = StringHelper.GetSizeInBytesFromString(maxImportSizeConfig);

        if (maxImportSize is null)
        {
            throw new MissingConfigurationException("Max import file size is invalid.");
        }

        builder.WebHost.UseKestrel(options =>
        {
            options.Limits.MaxRequestBodySize = maxImportSize;
        });
    }

    private static void ConfigureLogs(WebApplicationBuilder builder)
    {
        var elasticConfig = builder.Configuration.GetSection("ElasticSearch").Get<ElasticsearchConfig>();

        if (elasticConfig == null)
        {
            throw new MissingConfigurationException("ElasticSearch configuration is mandatory.");
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
            using (LockManager.GetWaitLock("MigrationWaitLock"))
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
        var controllersBuilder = builder.Services.AddControllers(options =>
        {
            options.Filters.Add<ValidateModelStateAttribute>();
        })
        .AddJsonOptions(opts =>
        {
            JsonHelper.Configure(opts.JsonSerializerOptions);
        });

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

    private static void ConfigureElasticSearch(WebApplicationBuilder builder)
    {
        var elasticConfig = builder.Configuration.GetSection("ElasticSearch").Get<ElasticsearchConfig>();

        if (elasticConfig == null)
        {
            throw new MissingConfigurationException("ElasticSearch configuration is mandatory.");
        }

        builder.Services.AddElasticsearch(elasticConfig);
    }

    private static void ConfigureIpDetailsResolver(WebApplicationBuilder builder)
    {
        var geolocationApiConfig = builder.Configuration.GetSection("GeolocationApi");

        if (geolocationApiConfig == null)
        {
            throw new MissingConfigurationException("Geo Location Api configuraiton is mandatory.");
        }

        builder.Services.Configure<GeolocationApiConfig>(geolocationApiConfig);
    }

    private static void ConfigureImageUpload(WebApplicationBuilder builder)
    {
        var imageUploadConfig = builder.Configuration.GetSection("Images");

        if (imageUploadConfig == null)
        {
            throw new MissingConfigurationException("Image Upload configuration is mandatory.");
        }

        builder.Services.Configure<ImagesConfig>(imageUploadConfig);
    }

    private static void ConfigureApiSettings(WebApplicationBuilder builder)
    {
        var apiSettingsConfig = builder.Configuration.GetSection("ApiSettings");

        if (apiSettingsConfig == null)
        {
            throw new MissingConfigurationException("Api settings configuraiton is mandatory.");
        }

        builder.Services.Configure<ApiSettingsConfig>(apiSettingsConfig);
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
            throw new MissingConfigurationException("Image Upload configuration is mandatory.");
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
        builder.Services.AddScoped<ITask, ContactScheduledEmailTask>();
        builder.Services.AddScoped<ITask, SyncIpDetailsTask>();
        builder.Services.AddScoped<ITask, SyncEsTask>();
        builder.Services.AddScoped<ITask, DomainCheckTask>();
    }

    private static void ConfigureCORS(WebApplicationBuilder builder)
    {
        var corsSettings = builder.Configuration.GetSection("Cors").Get<CorsConfig>();
        if (corsSettings == null)
        {
            throw new MissingConfigurationException("CORS configuration is mandatory.");
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