// <copyright file="OrdersItemsTests.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Text.Json;
using FluentAssertions;
using OnlineSales.DTOs;
using OnlineSales.Entities;

namespace OnlineSales.Tests;

public class OrdersItemsTests : TableWithFKTests<OrderItem, TestOrderItem, OrderItemUpdateDto>
{
    public OrdersItemsTests()
        : base("/api/order-items")
    {
    }

    [Fact]
    public async Task OrderQuantityTest()
    {
        var orderDetails = await CreateFKItem();

        int numberOfOrderItems = 10;

        int sumQuantity = 0;
        string[] orderItemsUrls = new string[numberOfOrderItems];

        for (var i = 0; i < numberOfOrderItems; i++)
        {
            var quantity = i + 1;
            sumQuantity += quantity;

            var testOrderItem = new TestOrderItem(string.Empty, orderDetails.Item1)
            {
                Quantity = quantity,
            };

            var newUrl = await PostTest(itemsUrl, testOrderItem);

            orderItemsUrls[i] = newUrl;
        }

        var updatedOrder = await GetTest<OrderDetailsDto>(orderDetails.Item2);

        updatedOrder.Should().NotBeNull();

        if (updatedOrder != null)
        {
            (updatedOrder.Quantity == sumQuantity).Should().BeTrue();
        }

        var addedOrderItem = await GetTest<OrderItem>(orderItemsUrls[0]);
        addedOrderItem.Should().NotBeNull();

        if (addedOrderItem != null)
        {
            var orderItem = new OrderItemUpdateDto
            {
                Quantity = addedOrderItem.Quantity + 999,
            };

            await PatchTest(orderItemsUrls[0], orderItem);

            updatedOrder = await GetTest<OrderDetailsDto>(orderDetails.Item2);
            updatedOrder.Should().NotBeNull();

            if (updatedOrder != null)
            {
                (updatedOrder.Quantity == sumQuantity + 999).Should().BeTrue();
            }
        }
    }

    [Fact]
    public async Task OrderTotalTest()
    {
        var orderDetails = await CreateFKItem();

        int numberOfOrderItems = 10;

        string[] orderItemsUrls = new string[numberOfOrderItems];

        for (var i = 0; i < numberOfOrderItems; ++i)
        {
            var orderItem = new TestOrderItem(string.Empty, orderDetails.Item1)
            {
                Quantity = i + 1,
            };

            var orderItemUrl = await PostTest(itemsUrl, orderItem);
            orderItemsUrls[i] = orderItemUrl;
        }

        async Task CompareTotals()
        {
            decimal total = 0m;
            foreach (var url in orderItemsUrls)
            {
                var orderItem = await GetTest<OrderItemDetailsDto>(url);
                orderItem.Should().NotBeNull();

                if (orderItem != null)
                {
                    total += orderItem.Total;
                }
            }

            var updatedOrder = await GetTest<OrderDetailsDto>(orderDetails.Item2);
            updatedOrder.Should().NotBeNull();

            if (updatedOrder != null)
            {
                updatedOrder.Total.Should().Be(total);
            }
        }

        await CompareTotals();

        var addedOrderItem = await GetTest<OrderItemDetailsDto>(orderItemsUrls[0]);
        addedOrderItem.Should().NotBeNull();

        var updatedOrderItem = new OrderItemUpdateDto();
        if (addedOrderItem != null)
        {
            updatedOrderItem.UnitPrice = addedOrderItem.UnitPrice;
            updatedOrderItem.Quantity = addedOrderItem.Quantity + 999;
        }

        await PatchTest(orderItemsUrls[0], updatedOrderItem);
        await CompareTotals();

        addedOrderItem = await GetTest<OrderItemDetailsDto>(orderItemsUrls[0]);
        addedOrderItem.Should().NotBeNull();
        updatedOrderItem = new OrderItemUpdateDto();
        if (addedOrderItem != null)
        {
            updatedOrderItem.UnitPrice = addedOrderItem.UnitPrice + new decimal(1.0);
            updatedOrderItem.Quantity = addedOrderItem.Quantity;
        }

        await PatchTest(orderItemsUrls[0], updatedOrderItem);
        await CompareTotals();
    }

    [Fact]
    public async Task OrderItemTotalTest()
    {
        var orderDetails = await CreateFKItem();

        var orderItem = new TestOrderItem(string.Empty, orderDetails.Item1);

        var orderItemUrl = await PostTest(itemsUrl, orderItem);

        var order = await GetTest<Order>(orderDetails.Item2);

        order.Should().NotBeNull();

        async Task CompareItems()
        {
            var addedOrderItem = await GetTest<OrderItemDetailsDto>(orderItemUrl);
            addedOrderItem.Should().NotBeNull();

            if (addedOrderItem != null)
            {
                addedOrderItem.CurrencyTotal.Should().Be(orderItem.Quantity * orderItem.UnitPrice);
                addedOrderItem.Total.Should().Be(addedOrderItem.CurrencyTotal * order!.ExchangeRate);
            }
        }

        await CompareItems();

        var addedOrderItem = await GetTest<OrderItemDetailsDto>(orderItemUrl);
        addedOrderItem.Should().NotBeNull();

        var updatedOrderItem = new OrderItemUpdateDto();

        if (addedOrderItem != null)
        {
            updatedOrderItem.Quantity = addedOrderItem.Quantity + 10;
            updatedOrderItem.UnitPrice = addedOrderItem.UnitPrice;
        }

        await PatchTest(orderItemUrl, orderItem);
        await CompareItems();

        addedOrderItem = await GetTest<OrderItemDetailsDto>(orderItemUrl);
        addedOrderItem.Should().NotBeNull();

        updatedOrderItem = new OrderItemUpdateDto();

        if (addedOrderItem != null)
        {
            updatedOrderItem.Quantity = addedOrderItem.Quantity;
            updatedOrderItem.UnitPrice = addedOrderItem.UnitPrice + new decimal(1.0);
        }

        await PatchTest(orderItemUrl, orderItem);
        await CompareItems();
    }

    [Fact]
    public async Task ShouldNotUpdateTotalsTest()
    {
        var addedOrderItemDetails = await CreateItem();
        var orderItemUrl = addedOrderItemDetails.Item2;
        var addedOrderItem = await GetTest<OrderItem>(orderItemUrl);
        addedOrderItem.Should().NotBeNull();

        if (addedOrderItem != null)
        {
            var updateOrderItemT = new TestOrderItemUpdateWithTotal();
            updateOrderItemT.Total = addedOrderItem.Total + 1.0m;
            await Patch(orderItemUrl, updateOrderItemT);

            var updatedOrderItem = await GetTest<OrderItem>(orderItemUrl);
            updatedOrderItem.Should().NotBeNull();

            if (updatedOrderItem != null)
            {
                updatedOrderItem.Total.Should().Be(addedOrderItem.Total);
            }

            var updateOrderItemCT = new TestOrderItemUpdateWithCurrencyTotal();
            updateOrderItemCT.CurrencyTotal = addedOrderItem.CurrencyTotal + 1.0m;
            await Patch(orderItemUrl, updateOrderItemCT);

            updatedOrderItem = await GetTest<OrderItem>(orderItemUrl);
            updatedOrderItem.Should().NotBeNull();

            if (updatedOrderItem != null)
            {
                updatedOrderItem.CurrencyTotal.Should().Be(addedOrderItem.CurrencyTotal);
            }
        }
    }

    [Theory]
    [InlineData("orderItems.csv", 3)]
    [InlineData("orderItems.json", 3)]
    public async Task ImportFileAddUpdateTest(string fileName, int expectedCount)
    {
        await CreateItem();
        await PostImportTest(itemsUrl, fileName);

        var allOrderItemsResponse = await GetTest(itemsUrl);
        allOrderItemsResponse.Should().NotBeNull();

        var content = await allOrderItemsResponse.Content.ReadAsStringAsync();
        var allOrderItems = JsonSerializer.Deserialize<List<OrderItem>>(content);
        allOrderItems.Should().NotBeNull();
        allOrderItems!.Count.Should().Be(expectedCount);
    }

    [Theory]
    [InlineData("orderItemsNoRef.csv", 1)]
    [InlineData("orderItemsNoRef.json", 1)]
    public async Task ImportFileNoOrderRefNotFoundTest(string fileName, int expectedCount)
    {
        await CreateItem();
        var importResult = await PostImportTest(itemsUrl, fileName, HttpStatusCode.OK);

        importResult.Added.Should().Be(0);
        importResult.Failed.Should().Be(1);

        var allOrderItemsResponse = await GetTest(itemsUrl);
        allOrderItemsResponse.Should().NotBeNull();

        var content = await allOrderItemsResponse.Content.ReadAsStringAsync();
        var allOrderItems = JsonSerializer.Deserialize<List<OrderItem>>(content);
        allOrderItems.Should().NotBeNull();
        allOrderItems!.Count.Should().Be(expectedCount);
    }

    protected override async Task<(TestOrderItem, string)> CreateItem(string uid, int fkId)
    {
        var testOrderItem = new TestOrderItem(uid, fkId);

        var newUrl = await PostTest(itemsUrl, testOrderItem);

        return (testOrderItem, newUrl);
    }

    protected override OrderItemUpdateDto UpdateItem(TestOrderItem to)
    {
        var from = new OrderItemUpdateDto();
        to.ProductName = from.ProductName = to.ProductName + "Updated";
        return from;
    }

    protected override async Task<(int, string)> CreateFKItem()
    {
        var contactCreate = new TestContact();

        var contactUrl = await PostTest("/api/contacts", contactCreate);

        var contact = await GetTest<Contact>(contactUrl);

        contact.Should().NotBeNull();

        var orderCreate = new TestOrder(string.Empty, contact!.Id);

        var orderUrl = await PostTest("/api/orders", orderCreate);

        var order = await GetTest<Order>(orderUrl);

        order.Should().NotBeNull();

        return (order!.Id, orderUrl);
    }
}