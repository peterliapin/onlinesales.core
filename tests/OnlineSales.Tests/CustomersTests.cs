// <copyright file="CustomersTests.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using FluentAssertions;
using OnlineSales.DTOs;
using OnlineSales.Entities;
using OnlineSales.Tests.TestEntities;

namespace OnlineSales.Tests;
public class CustomersTests : SimpleTableTests<Customer, TestCustomerCreate, TestCustomerUpdate>
{
    public CustomersTests()
        : base("/api/customers")
    {
    }

    protected override TestCustomerUpdate UpdateItem(TestCustomerCreate to)
    {
        var from = new TestCustomerUpdate();
        to.Email = from.Email ?? to.Email;
        return from;
    }
}