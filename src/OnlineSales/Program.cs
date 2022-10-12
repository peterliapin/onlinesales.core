using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Formatting.Json;
using Serilog.Sinks.Elasticsearch;
using Serilog.Sinks.File;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

ConfigureLogs();

builder.Host.UseSerilog();

Log.Logger.Information("Test log informaiton");

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

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

void ConfigureLogs()
{
    var configuration = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .Build();

    Log.Logger = new LoggerConfiguration()
        .Enrich.FromLogContext()
        .Enrich.WithExceptionDetails()
        .WriteTo.Debug()
        .WriteTo.Console()
        .WriteTo.Elasticsearch(ConfigureELK(configuration))
        .CreateLogger();
}

ElasticsearchSinkOptions ConfigureELK(IConfigurationRoot configuration)
{
    var uri = new Uri(configuration["ELKConfiguration:Uri"]);

    var assemblyName = Assembly.GetExecutingAssembly().GetName()
        !.Name!.ToLower();

    return new ElasticsearchSinkOptions(uri)
    {
        AutoRegisterTemplate = true,
        AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv7,
        IndexFormat = $"{assemblyName}-logs",
    };
}