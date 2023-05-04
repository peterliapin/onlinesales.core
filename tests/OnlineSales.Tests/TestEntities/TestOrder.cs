// <copyright file="TestOrder.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel.DataAnnotations;

namespace OnlineSales.Tests.TestEntities;

public class TestOrder : OrderCreateDto
{
    public TestOrder(string uid = "", int contactId = 0)
    {
        this.RefNo = $"1000{uid}";
        this.Currency = "USD";
        this.ExchangeRate = 1.234M;
        this.ContactId = contactId;
    }
}

public class TestOrderWithQuantity : TestOrder
{
    [Required]
    public int Quantity { get; set; } = 10;
}

public class TestOrderWithTotal : TestOrder
{
    [Required]
    public decimal Total { get; set; } = 10;
}

public class TestOrderWithCurrencyTotal : TestOrder
{
    [Required]
    public decimal CurrencyTotal { get; set; } = 10;
}