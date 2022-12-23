// <copyright file="OrdersTests.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using FluentAssertions;
using OnlineSales.DTOs;
using OnlineSales.Entities;

namespace OnlineSales.Tests;

public class OrdersTests : TableWithFKTests<Order, TestOrder, OrderUpdateDto>
{
    public OrdersTests()
        : base("/api/orders")
    {
    }

    [Fact]
    public override async Task UpdateItemNotFoundTest()
    {
        var updateOrder = new OrderUpdateDto();
        updateOrder.RefNo = "1111";
        await PatchTest(itemsUrlNotFound, updateOrder, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldNotBeUpdateTest()
    {
        var orderDetails = await CreateItem();

        var order = await GetTest<Order>(orderDetails.Item2);
        order.Should().NotBeNull();

        if (order != null)
        {
            var orderUrl = orderDetails.Item2;

            var updateOrderQ = new TestOrderWithQuantity();
            updateOrderQ.Quantity = order.Quantity + 10;
            await Patch(orderUrl, updateOrderQ);
            var updatedOrder = await GetTest<OrderItem>(orderUrl);
            updatedOrder.Should().NotBeNull();
            if (updatedOrder != null)
            {
                updatedOrder.Quantity.Should().Be(order.Quantity);
            }

            var updateOrderT = new TestOrderWithTotal();
            updateOrderT.Total = order.Total + new decimal(10);
            await Patch(orderUrl, updateOrderT);
            updatedOrder = await GetTest<OrderItem>(orderUrl);
            updatedOrder.Should().NotBeNull();
            if (updatedOrder != null)
            {
                updatedOrder.Total.Should().Be(order.Total);
            }

            var updateOrderCT = new TestOrderWithCurrencyTotal();
            updateOrderCT.CurrencyTotal = order.CurrencyTotal + new decimal(10);
            await Patch(orderUrl, updateOrderCT);
            updatedOrder = await GetTest<OrderItem>(orderUrl);
            updatedOrder.Should().NotBeNull();
            if (updatedOrder != null)
            {
                updatedOrder.CurrencyTotal.Should().Be(order.CurrencyTotal);
            }
        }
    }

    protected override async Task<(TestOrder, string)> CreateItem(int fkId)
    {
        var testOrder = new TestOrder
        {
            CustomerId = fkId,
        };

        var newUrl = await PostTest(itemsUrl, testOrder);

        return (testOrder, newUrl);
    }

    protected override OrderUpdateDto UpdateItem(TestOrder to)
    {
        var from = new OrderUpdateDto();
        to.RefNo = from.RefNo = to.RefNo + "1";
        return from;
    }

    protected override async Task<(int, string)> CreateFKItem()
    {
        var fkItemCreate = new TestCustomer();

        var fkUrl = await PostTest("/api/customers", fkItemCreate);

        var fkItem = await GetTest<Customer>(fkUrl);

        fkItem.Should().NotBeNull();

        return (fkItem!.Id, fkUrl);
    }
}