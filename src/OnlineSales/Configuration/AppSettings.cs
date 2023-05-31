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

public class ElasticConfig : BaseServiceConfig
{
    public bool UseHttps { get; set; } = false;

    public string IndexPrefix { get; set; } = string.Empty;

    public string Url => $"http{(UseHttps ? "s" : string.Empty)}://{Server}:{Port}";
}

public class SmtpServerConfig : BaseServiceConfig
{
    public bool UseTLS { get; set; } = false;
}

public class ExtensionConfig
{
    public string Extension { get; set; } = string.Empty;

    public string MaxSize { get; set; } = string.Empty;
}

public class MediaConfig
{
    public string[] Extensions { get; set; } = Array.Empty<string>();

    public ExtensionConfig[] MaxSize { get; set; } = Array.Empty<ExtensionConfig>();

    public string? CacheTime { get; set; }
}

public class EmailVerificationApiConfig
{
    public string Url { get; set; } = string.Empty;

    public string ApiKey { get; set; } = string.Empty;
}

public class AccountDetailsApiConfig
{
    public string Url { get; set; } = string.Empty;

    public string ApiKey { get; set; } = string.Empty;
}

public class ApiSettingsConfig
{
    public int MaxListSize { get; set; }

    public string MaxImportSize { get; set; } = string.Empty;

    public string Language { get; set; } = "ru-RU";
}

public class GeolocationApiConfig
{
    public string Url { get; set; } = string.Empty;

    public string AuthKey { get; set; } = string.Empty;
}

public class TaskConfig
{
    public bool Enable { get; set; }

    public string CronSchedule { get; set; } = string.Empty;

    public int RetryCount { get; set; }

    public int RetryInterval { get; set; }
}

public class TaskWithBatchConfig : TaskConfig
{
    public int BatchSize { get; set; }
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

    public ElasticConfig Elastic { get; set; } = new ElasticConfig();

    public SmtpServerConfig SmtpServer { get; set; } = new SmtpServerConfig();
}

public class CorsConfig
{
    public string[] AllowedOrigins { get; set; } = Array.Empty<string>();
}

public class EmailConfig : BaseServiceConfig
{
    public bool UseSsl { get; set; }
}
