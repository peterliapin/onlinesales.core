// <copyright file="SmsLog.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OnlineSales.DataAnnotations;
using OnlineSales.Entities;

namespace OnlineSales.Plugin.Sms.Entities;

[Table("sms_log")]
[SupportsElastic]
[SupportsChangeLog]

public class SmsLog : BaseCreateByEntity
{
    public enum SmsStatus
    {
        NotSent = 0,
        Sent = 1,
    }

    [Required]
    public string Sender { get; set; } = string.Empty;

    [Required]
    public string Recipient { get; set; } = string.Empty;

    [Required]
    public string Message { get; set; } = string.Empty;

    [Required]
    public SmsStatus Status { get; set; }
}