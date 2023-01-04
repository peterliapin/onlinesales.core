// <copyright file="TestBulkOrderItems.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using Bogus;
using OnlineSales.Entities;

namespace OnlineSales.Tests.TestEntities.BulkPopulate
{
    public class TestBulkOrderItems : OrderItem
    {
        public List<OrderItem> GenerateBulk(int orderId, int count)
        {
            var fakeCustomer = new Faker<OrderItem>().
                RuleFor(c => c.UnitPrice, f => f.Random.Number(3)).
                RuleFor(c => c.Quantity, 1).
                RuleFor(c => c.LicenseCode, f => f.Random.AlphaNumeric(7)).
                RuleFor(c => c.Currency, "USD").
                RuleFor(c => c.OrderId, orderId);

            return fakeCustomer.Generate(count);
        }
    }
}

