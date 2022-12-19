// <copyright file="CustomerEmailSchedule.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace OnlineSales.Entities
{
    public enum ScheduleStatus
    {
        Pending = 0,
        Completed = 1,
    }

    [Table("customer_email_schedule")]
    public class CustomerEmailSchedule : BaseEntity
    {
        /// <summary>
        /// Gets or sets reference to the customer table.
        /// </summary>
        [Required]
        public int CustomerId { get; set; }

        [JsonIgnore]
        [ForeignKey("CustomerId")]
        public virtual Customer? Customer { get; set; }

        /// <summary>
        /// Gets or sets reference to the EmailSchedule table.
        /// </summary>
        [Required]
        public int ScheduleId { get; set; }

        [JsonIgnore]
        [ForeignKey("ScheduleId")]
        public EmailSchedule? Schedule { get; set; }

        /// <summary>
        /// Gets or sets the status of the completion of sending all emails assigned to the schedule.
        /// </summary>
        public ScheduleStatus Status { get; set; }
    }
}