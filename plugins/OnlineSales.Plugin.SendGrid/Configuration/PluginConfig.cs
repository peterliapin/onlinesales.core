// <copyright file="PluginConfig.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using OnlineSales.Configuration;

namespace OnlineSales.Plugin.SendGrid.Configuration;

public class PluginConfig
{
    public SendGridApiConfig SendGridApi { get; set; } = new SendGridApiConfig();
}

public class SendGridApiConfig
{
    public string ApiKey { get; set; } = string.Empty;
}