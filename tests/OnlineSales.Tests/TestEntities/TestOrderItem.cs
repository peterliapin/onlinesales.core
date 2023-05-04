// <copyright file="TestOrderItem.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel.DataAnnotations;

namespace OnlineSales.Tests.TestEntities;

public class TestOrderItem : OrderItemCreateDto
{
    public TestOrderItem(string uid = "", int orderId = 0)
    {
        this.Currency = "USD";
        this.LicenseCode = $"LicenseCode{uid}";
        this.ProductName = $"ProductName{uid}";
        this.UnitPrice = 1.99m;
        this.Quantity = 1;
        this.OrderId = orderId;
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