// <copyright file="TestOrder.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel.DataAnnotations;
using OnlineSales.DTOs;
using OnlineSales.Entities;

namespace OnlineSales.Tests.TestEntities;

public class TestOrderCreate : OrderCreateDto
{
    public TestOrderCreate()
    {
        RefNo = "1000";
        Currency = "USD";
        ExchangeRate = 2;
    }
}

public class TestOrderUpdate : OrderUpdateDto
{
    public TestOrderUpdate()
    {
        RefNo = "1001";
    }
}

public class TestOrderWithQuantity : TestOrderCreate
{
    [Required]
    public int Quantity { get; set; } = 10;
}

public class TestOrderWithTotal : TestOrderCreate
{
    [Required]
    public decimal Total { get; set; } = 10;
}

public class TestOrderWithCurrencyTotal : TestOrderCreate
{
    [Required]
    public decimal CurrencyTotal { get; set; } = 10;
}
