// <copyright file="TestOrderItem.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel.DataAnnotations;
using OnlineSales.DTOs;

namespace OnlineSales.Tests.TestEntities;

public class TestOrderItem : OrderItemCreateDto
{
    public TestOrderItem(string uid = "", int orderId = 0)
    {
        Currency = "USD";
        LicenseCode = $"LicenseCode{uid}";
        ProductName = $"ProductName{uid}";
        UnitPrice = 1.99m;
        Quantity = 1;
        OrderId = orderId;
    }
}

public class TestOrderItemUpdateWithTotal : OrderItemUpdateDto
{
    [Required]
    public decimal Total { get; set; } = 0;
}

public class TestOrderItemUpdateWithCurrencyTotal : OrderItemUpdateDto
{
    [Required]
    public decimal CurrencyTotal { get; set; } = 0;
}