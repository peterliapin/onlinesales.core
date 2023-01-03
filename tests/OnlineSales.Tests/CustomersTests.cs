// <copyright file="CustomersTests.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using OnlineSales.DTOs;
using OnlineSales.Entities;

namespace OnlineSales.Tests;
public class CustomersTests : SimpleTableTests<Customer, TestCustomer, CustomerUpdateDto>
{
    public CustomersTests()
        : base("/api/customers")
    {
    }

    protected override CustomerUpdateDto UpdateItem(TestCustomer to)
    {
        var from = new CustomerUpdateDto();
        to.Email = from.Email = "Updated" + to.Email;
        return from;
    }
}