// <copyright file="AppSettings.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

namespace OnlineSales.Configuration;

public class BaseServiceConfig
{
    public string Server { get; set; } = string.Empty;

    public int Port { get; set; } = 0;

    public string UserName { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
}

public class PostgresConfig : BaseServiceConfig
{
    public string Database { get; set; } = string.Empty;

    public string ConnectionString => $"User ID={UserName};Password={Password};Server={Server};Port={Port};Database={Database};Pooling=true;";
}

public class ElasticsearchConfig : BaseServiceConfig
{
    public bool UseHttps { get; set; } = false;

    public string IndexPrefix { get; set; } = string.Empty;

    public string Url => $"http{(UseHttps ? "s" : string.Empty)}://{Server}:{Port}";
}

public class SmtpServerConfig : BaseServiceConfig
{
    public bool UseTLS { get; set; } = false;
}

public class ImagesConfig
{
    public string[] Extensions { get; set; } = Array.Empty<string>();

    public string? MaxSize { get; set; }

    public string? CacheTime { get; set; }
}

public class ApiSettingsConfig
{
    public int MaxListSize { get; set; }

    public string MaxImportSize { get; set; } = string.Empty;
}

public class GeolocationApiConfig
{
    public string Url { get; set; } = string.Empty;

    public string AuthKey { get; set; } = string.Empty;
}

public class TaskConfig
{
    public string CronSchedule { get; set; } = string.Empty;

    public int RetryCount { get; set; }

    public int RetryInterval { get; set; }
}

public class DomainCheckTaskConfig : TaskConfig
{
    public DomainCheckTaskConfig()
    {
        CronSchedule = "* 0/5 * * * ?";

        RetryCount = 2;

        RetryInterval = 5;
    }

    public int BatchSize { get; } = 100;
}

public class CacheProfileSettings
{
    public string Type { get; set; } = string.Empty;

    public string VaryByHeader { get; set; } = string.Empty;

    public int? Duration { get; set; }
}

public class AppSettings
{
    public PostgresConfig Postgres { get; set; } = new PostgresConfig();

    public ElasticsearchConfig ElasticSearch { get; set; } = new ElasticsearchConfig();

    public SmtpServerConfig SmtpServer { get; set; } = new SmtpServerConfig();
}

public class CorsConfig
{
    public string[] AllowedOrigins { get; set; } = Array.Empty<string>();
}