// <copyright file="TestBulkOrders.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using Bogus;
using OnlineSales.Entities;

namespace OnlineSales.Tests.TestEntities.BulkPopulate
{
    public class TestBulkOrders : Order
    {
        public List<Order> GenerateBulk(int customerId, int count)
        {
            var fakeCustomer = new Faker<Order>().
                RuleFor(c => c.RefNo, f => f.Random.AlphaNumeric(5)).
                RuleFor(c => c.ExchangeRate, 1).
                RuleFor(c => c.AffiliateName, f => f.Company.CompanyName()).
                RuleFor(c => c.CustomerId, customerId);               

            return fakeCustomer.Generate(count);
        }
    }
}
