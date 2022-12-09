// <copyright file="TestOrderItem.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using OnlineSales.DTOs;
using OnlineSales.Entities;

namespace OnlineSales.Tests.TestEntities;

public class TestOrderItem : OrderItemCreateDto
{
    public TestOrderItem()
    {
        Currency = "USD";
        LicenseCode = "LicenseCode";
        ProductName = "ProductName";
        UnitPrice = 1;
    }
}

public class TestOrderItemUpdate : OrderItemUpdateDto
{
    public TestOrderItemUpdate()
    {
        LicenseCode = "LicenseCode";
        ProductName = "ProductName";
        Quantity = 1;
        UnitPrice = 1;
    }
}