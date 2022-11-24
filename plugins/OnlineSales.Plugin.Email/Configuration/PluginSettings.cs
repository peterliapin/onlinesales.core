// <copyright file="PluginSettings.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

namespace OnlineSales.Plugin.Email.Configuration;

public class PluginSettings
{
    public EmailConfig Email { get; set; } = new EmailConfig();
}

public class EmailConfig
{
    public string SmtpServer { get; set; } = string.Empty;

    public int Port { get; set; }

    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public bool UseSsl { get; set; }
}
