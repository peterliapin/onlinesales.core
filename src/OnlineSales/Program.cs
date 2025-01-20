// <copyright file="Program.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using OnlineSales.Configuration;
using OnlineSales.Controllers;
using OnlineSales.Data;
using OnlineSales.Entities;
using OnlineSales.Filters;
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

    public static async Task Main(string[] args)
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
        builder.Services.AddTransient<IMxVerifyService, MxVerifyService>();
        builder.Services.AddTransient<IIdentityService, IdentityService>();
        builder.Services.AddTransient<IDomainService, DomainService>();
        builder.Services.AddTransient<IOrderItemService, OrderItemService>();
        builder.Services.AddTransient<IContactService, ContactService>();
        builder.Services.AddTransient<ICommentService, CommentService>();
        builder.Services.AddScoped<IVariablesService, VariablesService>();
        builder.Services.AddSingleton<IpDetailsService, IpDetailsService>();
        builder.Services.AddSingleton<ILockService, LockService>();
        builder.Services.AddScoped<IEmailVerifyService, EmailVerifyService>();
        builder.Services.AddScoped<IEmailValidationExternalService, EmailValidationExternalService>();
        builder.Services.AddScoped<IAccountExternalService, AccountExternalService>();
        builder.Services.AddSingleton<TaskStatusService, TaskStatusService>();
        builder.Services.AddSingleton<ActivityLogService, ActivityLogService>();
        builder.Services.AddTransient(typeof(QueryProviderFactory<>), typeof(QueryProviderFactory<>));
        builder.Services.AddTransient(typeof(ESOnlyQueryProviderFactory<>), typeof(ESOnlyQueryProviderFactory<>));
        builder.Services.AddSingleton<IEmailService, EmailService>();
        builder.Services.AddTransient<CommentableControllerExtension, CommentableControllerExtension>();
        builder.Services.AddScoped<IDealService, DealService>();
        builder.Services.AddTransient<IOrderService, OrderService>();
        builder.Services.AddTransient<IDiscountService, DiscountService>();
        builder.Services.AddTransient<IEmailSchedulingService, EmailSchedulingService>();

        ConfigureCacheProfiles(builder);

        ConfigureConventions(builder);
        IdentityHelper.ConfigureAuthentication(builder);
        ConfigureControllers(builder);

        builder.Services.AddDbContext<PgDbContext>();
        builder.Services.AddSingleton<EsDbContext>();

        ConfigureQuartz(builder);
        ConfigureImageUpload(builder);
        ConfigureFileUpload(builder);
        ConfigureIpDetailsResolver(builder);
        ConfigureEmailServices(builder);
        ConfigureTasks(builder);
        ConfigureApiSettings(builder);
        ConfigureImportSizeLimit(builder);
        ConfigureEmailVerification(builder);
        ConfigureAccountDetails(builder);
        ConfigureIdentity(builder);

        builder.Services.AddAutoMapper(x =>
        {
            x.AddProfile(new AutoMapperProfiles());
            x.AllowNullCollections = true;
        });

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

        await MigrateOnStartIfRequired(app, builder);

        app.UseSwagger();
        app.UseSwaggerUI();
        app.UseDefaultFiles();
        app.UseStaticFiles();
        app.UseCors();

        app.UseAuthentication();
        app.UseAuthorization();

        PluginManager.Init(app);

        app.UseCookiePolicy();
        app.MapControllers();

        app.UseSpa(spa =>
        {
            // works out of the box, no configuration required
        });

        app.Run();
    }

    public static async Task CreateDefaultIdentity(IServiceScope scope)
    {
        var defaultRoles = app!.Configuration.GetSection("DefaultRoles").Get<DefaultRolesConfig>()!;

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        foreach (var defaultRole in defaultRoles)
        {
            if (!await roleManager.RoleExistsAsync(defaultRole))
            {
                await roleManager.CreateAsync(new IdentityRole(defaultRole));
            }
        }

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

        var defaultUsers = app!.Configuration.GetSection("DefaultUsers").Get<DefaultUsersConfig>()!;

        foreach (var defaultUser in defaultUsers)
        {
            var user = new User
            {
                CreatedAt = DateTime.UtcNow,
                DisplayName = defaultUser.UserName,
                UserName = defaultUser.UserName,
                Email = defaultUser.Email,
                EmailConfirmed = true,
            };

            var existingUser = await userManager.FindByEmailAsync(user.Email);

            if (existingUser == null)
            {
                var result = await userManager.CreateAsync(user, defaultUser.Password);

                if (result.Succeeded)
                {
                    await userManager.AddToRolesAsync(user, defaultUser.Roles);
                }
            }
        }
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
        var elasticConfig = builder.Configuration.GetSection("Elastic").Get<ElasticConfig>();

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

    private static ElasticsearchSinkOptions ConfigureELK(ElasticConfig elasticConfig)
    {
        var uri = new Uri(elasticConfig.Url);

        return new ElasticsearchSinkOptions(uri)
        {
            AutoRegisterTemplate = true,
            AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv7,
            IndexFormat = $"{elasticConfig.IndexPrefix}-logs",
        };
    }

    private static async Task MigrateOnStartIfRequired(WebApplication app, WebApplicationBuilder builder)
    {
        var migrateOnStart = builder.Configuration.GetValue<bool>("MigrateOnStart");

        if (migrateOnStart)
        {
            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<PgDbContext>();

                using (LockManager.GetWaitLock("MigrationWaitLock", context.Database.GetConnectionString()!))
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<PgDbContext>();
                    dbContext.Database.Migrate();

                    var esDbContext = scope.ServiceProvider.GetRequiredService<EsDbContext>();
                    esDbContext.Migrate();

                    var pluginContexts = scope.ServiceProvider.GetServices<PluginDbContextBase>();

                    foreach (var pluginContext in pluginContexts)
                    {
                        pluginContext.Database.Migrate();
                    }

                    // var elasticClient = scope.ServiceProvider.GetRequiredService<ElasticClient>();

                    await CreateDefaultIdentity(scope);
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

    private static void ConfigureIpDetailsResolver(WebApplicationBuilder builder)
    {
        var geolocationApiConfig = builder.Configuration.GetSection("GeolocationApi");

        if (geolocationApiConfig == null)
        {
            throw new MissingConfigurationException("Geo Location Api configuration is mandatory.");
        }

        builder.Services.Configure<GeolocationApiConfig>(geolocationApiConfig);
    }

    private static void ConfigureImageUpload(WebApplicationBuilder builder)
    {
        var imageUploadConfig = builder.Configuration.GetSection("Media");

        if (imageUploadConfig == null)
        {
            throw new MissingConfigurationException("Image Upload configuration is mandatory.");
        }

        builder.Services.Configure<MediaConfig>(imageUploadConfig);
    }

    private static void ConfigureFileUpload(WebApplicationBuilder builder)
    {
        var fileUploadConfig = builder.Configuration.GetSection("File");

        if (fileUploadConfig == null)
        {
            throw new MissingConfigurationException("File Upload configuration is mandatory.");
        }

        builder.Services.Configure<FileConfig>(fileUploadConfig);
    }

    private static void ConfigureEmailVerification(WebApplicationBuilder builder)
    {
        var emailVerificationConfig = builder.Configuration.GetSection("EmailVerificationApi");

        if (emailVerificationConfig == null)
        {
            throw new MissingConfigurationException("Email Verification Api configuration is mandatory.");
        }

        builder.Services.Configure<EmailVerificationApiConfig>(emailVerificationConfig);
    }

    private static void ConfigureIdentity(WebApplicationBuilder builder)
    {
        var jwtConfig = builder.Configuration.GetSection("Jwt");

        if (jwtConfig == null)
        {
            throw new MissingConfigurationException("Jwt configuration is mandatory.");
        }

        builder.Services.Configure<JwtConfig>(jwtConfig);
    }

    private static void ConfigureAccountDetails(WebApplicationBuilder builder)
    {
        var accountDetailsApiConfig = builder.Configuration.GetSection("AccountDetailsApi");

        if (accountDetailsApiConfig == null)
        {
            throw new MissingConfigurationException("Account Details Api configuration is mandatory.");
        }

        builder.Services.Configure<AccountDetailsApiConfig>(accountDetailsApiConfig);
    }

    private static void ConfigureApiSettings(WebApplicationBuilder builder)
    {
        var apiSettingsConfig = builder.Configuration.GetSection("ApiSettings");

        if (apiSettingsConfig == null)
        {
            throw new MissingConfigurationException("Api settings configuration is mandatory.");
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

            config.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
            {
                Description = "Copy 'Bearer ' + valid JWT token into field",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer",
            });

            config.AddSecurityRequirement(new OpenApiSecurityRequirement()
            {
                {
                    new OpenApiSecurityScheme()
                    {
                        Reference = new OpenApiReference()
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer",
                        },
                        Scheme = "oauth2",
                        Name = "Bearer",
                        In = ParameterLocation.Header,
                    },
                    new List<string>()
                },
            });

            config.EnableAnnotations();

            config.SupportNonNullableReferenceTypes();

            config.SchemaFilter<CustomSwaggerScheme>();

            config.UseInlineDefinitionsForEnums();

            config.SwaggerDoc("v1", openApiInfo);

            var conf = builder.Configuration.GetSection("Entities").Get<EntitiesConfig>();
            config.DocumentFilter<SwaggerEntitiesFilter>(conf);
        });
    }

    private static void ConfigureQuartz(WebApplicationBuilder builder)
    {
        var taskRunnerSchedule = builder.Configuration.GetValue<string>("TaskRunner:CronSchedule")!;

        builder.Services.AddQuartz(q =>
        {
            // q.UseMicrosoftDependencyInjectionJobFactory(); -- obsolete and no longer needed

            q.AddJob<TaskRunner>(opts => opts.WithIdentity("TaskRunner"));

            q.AddTrigger(opts =>
                opts.ForJob("TaskRunner").WithIdentity("TaskRunner").WithCronSchedule(taskRunnerSchedule));
        });

        builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

        builder.Services.AddTransient<TaskRunner>();
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
        builder.Services.AddScoped<ITask, SyncEsTask>();
        builder.Services.AddScoped<ITask, SyncIpDetailsTask>();
        builder.Services.AddScoped<ITask, DomainVerificationTask>();
        builder.Services.AddScoped<ITask, ContactScheduledEmailTask>();
        builder.Services.AddScoped<ITask, ContactAccountTask>();
        builder.Services.AddScoped<ITask, SyncEmailLogTask>();
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
                    .AllowCredentials()
                    .AllowAnyHeader();
                if (builder.Environment.IsDevelopment())
                {
                    policy.SetIsOriginAllowed(origin => true);
                    return;
                }

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