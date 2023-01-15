// <copyright file="TestOrder.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel.DataAnnotations;
using OnlineSales.DTOs;

namespace OnlineSales.Tests.TestEntities;

public class TestOrder : OrderCreateDto
{
    public TestOrder(string uid = "", int contactId = 0)
    {
        RefNo = $"1000{uid}";
        Currency = "USD";
        ExchangeRate = 2;
        ContactId = contactId;
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