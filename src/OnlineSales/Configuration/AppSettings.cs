// <copyright file="AppSettings.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

namespace OnlineSales.Configuration;

public class EntitiesConfig
{
    public string[] Include { get; set; } = Array.Empty<string>();

    public string[] Exclude { get; set; } = Array.Empty<string>();
}

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

public class FileConfig
{
    public string[] Extensions { get; set; } = Array.Empty<string>();

    public ExtensionConfig[] MaxSize { get; set; } = Array.Empty<ExtensionConfig>();
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

    public string DefaultLanguage { get; set; } = "en-US";
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
}

public class CorsConfig
{
    public string[] AllowedOrigins { get; set; } = Array.Empty<string>();
}

public class EmailConfig : BaseServiceConfig
{
    public bool UseSsl { get; set; }
}

public class JwtConfig
{
    public string Issuer { get; set; } = string.Empty;

    public string Audience { get; set; } = string.Empty;

    public string Key { get; set; } = string.Empty;
}

public class AzureADConfig
{
    public string Instance { get; set; } = string.Empty;

    public string TenantId { get; set; } = string.Empty;

    public string Domain { get; set; } = string.Empty;

    public string ClientId { get; set; } = string.Empty;

    public string ClientSecret { get; set; } = string.Empty;
}