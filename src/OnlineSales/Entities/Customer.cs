// <copyright file="Customer.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel.DataAnnotations;

namespace OnlineSales.Entities
{
    public class Customer : BaseEntity
    {
        public string LastName { get; set; } = string.Empty;

        [Required]
        public string FirstName { get; set; } = string.Empty;

        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string CompanyName { get; set; } = string.Empty;

        public string Address1 { get; set; } = string.Empty;

        public string Address2 { get; set; } = string.Empty;

        public string State { get; set; } = string.Empty;

        public string Zip { get; set; } = string.Empty;

        public string Location { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

       // public TimeZoneInfo Timezone { get; set; } = default!;

        public string Culture { get; set; } = string.Empty;
    }
}
