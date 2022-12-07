// <copyright file="PluginSettings.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

namespace OnlineSales.Plugin.Vsto.Configuration;

public class PluginConfig
{
    public VstoConfig Vsto { get; set; } = new VstoConfig();
}

public class VstoConfig
{
    public string RequestPath { get; set; } = string.Empty;

    public string LocalPath { get; set; } = string.Empty;
}