// <copyright file="EmailTemplate.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace OnlineSales.Entities
{
    [Table("email_template")]
    public class EmailTemplate : BaseEntity
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Subject { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the HTML template body of the email.
        /// </summary>
        [Required]
        public string BodyTemplate { get; set; } = string.Empty;

        [Required]
        public string FromEmail { get; set; } = string.Empty;

        [Required]
        public string FromName { get; set; } = string.Empty;

        [Required]
        public int GroupId { get; set; }

        [JsonIgnore]
        [ForeignKey("GroupId")]
        public virtual EmailGroup? Group { get; set; }
    }
}
