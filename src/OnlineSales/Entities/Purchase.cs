// <copyright file="Purchase.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

namespace OnlineSales.Entities
{
    public class Purchase : BaseEntity
    {
        public string? License { get; set; } = string.Empty;

        public string? OrderId { get; set; } = string.Empty;

        public int Quatity { get; set; } = 0;

        public decimal Total { get; set; } = 0;

        public string Product { get; set; } = string.Empty;

        public string ShipCountry { get; set; } = string.Empty;

        public string ShipState { get; set; } = string.Empty;

        public string Item { get; set; } = string.Empty;

        public string Url { get; set; } = string.Empty;

        public virtual Customer? Customer { get; set; }

        public int? CustomerId { get; set; }
    }
}
