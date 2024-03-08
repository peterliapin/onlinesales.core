// <copyright file="EmailLog.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using OnlineSales.DataAnnotations;

namespace OnlineSales.Entities
{
    public enum EmailStatus
    {
        NotSent = 0,
        Sent = 1,
        Received = 2,
    }

    [SupportsElastic]
    [SupportsChangeLog]
    [Table("email_log")]
    public class EmailLog : BaseEntity
    {
        private string fromEmail = string.Empty;
        private string recipients = string.Empty;

        /// <summary>
        /// Gets or sets reference to the ContactEmailSchedule table.
        /// </summary>
        public int? ScheduleId { get; set; }

        /// <summary>
        /// Gets or sets reference to the contact table.
        /// </summary>
        public int? ContactId { get; set; }

        [JsonIgnore]
        [ForeignKey("ContactId")]
        public virtual Contact? Contact { get; set; }

        /// <summary>
        /// Gets or sets reference to the EmailTemplate table.
        /// </summary>
        public int? TemplateId { get; set; }

        [Searchable]
        [Required(AllowEmptyStrings = true)]
        public string Subject { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the email address of the email recipient.
        /// </summary>
        [Searchable]
        [Required]
        public string Recipients
        {
            get
            {
                return recipients;
            }

            set
            {
                recipients = value.ToLower();
            }
        }

        [Required]
        [Searchable]
        public string FromEmail
        {
            get
            {
                return fromEmail;
            }

            set
            {
                fromEmail = value.ToLower();
            }
        }

        /// <summary>
        /// Gets or sets the email body.
        /// </summary>
        [Required]
        [Searchable]        
        public string HtmlBody { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the email body.
        /// </summary>        
        [Searchable]        
        public string? TextBody { get; set; }

        public string MessageId { get; set; } = string.Empty;

        public EmailStatus Status { get; set; }
    }
}