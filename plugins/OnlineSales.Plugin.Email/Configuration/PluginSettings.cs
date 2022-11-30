// <copyright file="PluginSettings.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using OnlineSales.Configuration;

namespace OnlineSales.Plugin.Email.Configuration;

public class PluginSettings
{
    public EmailConfig Email { get; set; } = new EmailConfig();
}

public class EmailConfig : BaseServiceConfig
{
    public bool UseSsl { get; set; }
}
