// <copyright file="VstoUserVersion.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineSales.Plugin.Vsto.Entities;

[Table("vsto_user_version")]
public class VstoUserVersion
{
    public int Id { get; set; }

    public string IpAddress { get; set; } = string.Empty;

    public string Version { get; set; } = string.Empty;

    public DateTime ExpireDateTime { get; set; }
}

