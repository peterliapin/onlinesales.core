// <copyright file="EmailTemplate.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace OnlineSales.Entities
{
    public class EmailTemplate : BaseEntity
    {
        [Required]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the HTML template body of the email.
        /// </summary>
        [Required]
        public string Template { get; set; } = string.Empty;

        [Required]
        public int EmailGroupId { get; set; }

        [JsonIgnore]
        [ForeignKey("EmailGroupId")]
        public virtual EmailGroup? EmailGroup { get; set; }
    }
}
