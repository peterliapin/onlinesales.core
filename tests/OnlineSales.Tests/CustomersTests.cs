// <copyright file="CustomersTests.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Formatters;
using OnlineSales.DTOs;
using OnlineSales.Entities;
using OnlineSales.Tests.TestEntities;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace OnlineSales.Tests;
public class CustomersTests : SimpleTableTests<Customer, TestCustomer, CustomerUpdateDto, CustomerDetailsDto>
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