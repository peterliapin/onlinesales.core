// <copyright file="TestCustomer.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using OnlineSales.DTOs;

namespace OnlineSales.Tests.TestEntities;

public class TestCustomerCreate : CustomerCreateDto
{
    public TestCustomerCreate()
    {
        Email = "customer@test.net";
    }
}

public class TestCustomerUpdate : CustomerUpdateDto
{
    public TestCustomerUpdate()
    {
        Email = "customerUpdate@test.net";
    }
}

public class TestCustomerConverter : ITestTypeConverter<TestCustomerUpdate, TestCustomerCreate>
{
    public void Convert(TestCustomerUpdate from, TestCustomerCreate to)
    {
        to.Email = from.Email ?? to.Email;
    }
}
