// <copyright file="TestBulkCustomers.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bogus;
using OnlineSales.Entities;

namespace OnlineSales.Tests.TestEntities.BulkPopulate
{
    public class TestBulkCustomers : Customer
    {
        public List<Customer> GenerateBulk(int count)
        {
            var fakeCustomer = new Faker<Customer>().
                RuleFor(c => c.FirstName, f => f.Name.FirstName()).
                RuleFor(c => c.Address1, f => f.Address.StreetAddress()).
                RuleFor(c => c.CompanyName, f => f.Company.CompanyName()).
                RuleFor(c => c.Email, f => f.Internet.Email());

            return fakeCustomer.Generate(count);   
        }
    }
}
