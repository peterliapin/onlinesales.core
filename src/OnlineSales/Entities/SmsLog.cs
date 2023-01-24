// <copyright file="SmsLog.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OnlineSales.DataAnnotations;

namespace OnlineSales.Entities
{
    [Table("sms_log")]
    [SupportsElasticSearch]
    [SupportsChangeLog]
    public class SmsLog : BaseEntity
    {
        [Required]
        public string Recipient { get; set; } = string.Empty;

        [Required]
        public string Message { get; set; } = string.Empty;
    }
}