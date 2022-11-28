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
        /// <summary>
        /// Gets or sets reference to the CustomerEmailSchedule table.
        /// </summary>
        [Required]
        public int ScheduleId { get; set; }

        [JsonIgnore]
        [ForeignKey("CustomerEmailScheduleId")]
        public CustomerEmailSchedule? Schedule { get; set; }

        /// <summary>
        /// Gets or sets reference to the EmailTemplate table.
        /// </summary>
        [Required]
        public int TemplateId { get; set; }

        [JsonIgnore]
        [ForeignKey("EmailTemplateId")]
        public EmailTemplate? Template { get; set; }

        public EmailStatus Status { get; set; }
    }
}
