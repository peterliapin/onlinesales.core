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
        PENDING = 0,
        COMPLETED = 1,
    }

    public class CustomerEmailSchedule : BaseEntity
    {
        /// <summary>
        /// Gets or sets reference to a customer table.
        /// </summary>
        [Required]
        public int CustomerId { get; set; }

        [JsonIgnore]
        [ForeignKey("CustomerId")]
        public virtual Customer? Customer { get; set; }

        /// <summary>
        /// Gets or sets reference to EmailSchedule table.
        /// </summary>
        [Required]
        public int EmaiScheduleId { get; set; }

        [JsonIgnore]
        [ForeignKey("EmailScheduleId")]
        public EmailSchedule? EmailSchedule { get; set; }

        /// <summary>
        /// Gets or sets the status of the completion of sending all emails assigned to the schedule.
        /// </summary>
        public ScheduleStatus Status { get; set; }
    }
}
