// <copyright file="CustomerEmailLog.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace OnlineSales.Entities
{
    public enum EmailStatus
    {
        NOTSENT = 0,
        SENT = 1,
    }

    public class CustomerEmailLog : BaseEntity
    {
        public int CustomerEmailScheduleId { get; set; }

        [JsonIgnore]
        [ForeignKey("CustomerEmailScheduleId")]
        public CustomerEmailSchedule? EmailSchedule { get; set; }

        [Required]
        public int EmailTemplateId { get; set; }

        [JsonIgnore]
        [ForeignKey("EmailTemplateId")]
        public EmailTemplate? Email { get; set; }

        public EmailStatus EmailStatus { get; set; }
    }
}
