// <copyright file="OrdersTests.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System;
using System.Formats.Asn1;
using System.Security.Policy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Namotion.Reflection;
using NJsonSchema.Validation;
using OnlineSales.DTOs;
using OnlineSales.Entities;

namespace OnlineSales.Tests;

public class OrdersTests : BaseTest
{
    private static readonly string UrlOrders = "/api/orders";
    private static readonly string UrlOrdersNotFound = UrlOrders + "/404";
    private static readonly string UrlOrderItems = "/api/orderitems";

    [Fact]
    public async Task GetOrderNotFoundTest()
    {
        await GetTest(UrlOrdersNotFound, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateOrderWithNonExistedCustomer()
    {
        var customers = await GetTest<Customer[]>("/api/customers");
        customers.Should().NotBeNull();

        int maxId = 0;
        if (customers != null)
        {
            foreach (var c in customers)
            {
                if (c.Id > maxId)
                {
                    maxId = c.Id;
                }
            }
        }

        var testOrder = new TestOrder();
        testOrder.CustomerId = maxId + 1;
        await UnsuccessfulPostTest(UrlOrders, testOrder);
    }

    [Fact]
    public async Task CreateAndGetOrderTest()
    {
        var testOrder = AddCustomerAndCreateOrder();

        var newOrderUrl = await PostTest(UrlOrders, testOrder);

        var order = await GetTest<Order>(newOrderUrl);

        order.Should().BeEquivalentTo(testOrder);
    }

    [Fact]
    public async Task UpdateOrderNotFoundTest()
    {
        var order = new TestOrder();
        await PatchTest(UrlOrdersNotFound, order, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task OrderQuantityTest()
    {
        var order = AddOrder();

        int numberOfOrderItems = 10;

        var random = new Random();
        int sumQuantity = 0;
        string[] orderItemsUrls = new string[numberOfOrderItems];
        for (var i = 0; i < numberOfOrderItems; ++i)
        {
            var quantity = random.Next(1, 1000);
            sumQuantity += quantity;
            orderItemsUrls[i] = AddOrderItem(order.Item1, quantity);
        }

        var updatedOrder = await GetTest<Order>(order.Item2);

        updatedOrder.Should().NotBeNull();

        if (updatedOrder != null)
        {
            (updatedOrder.Quantity == sumQuantity).Should().BeTrue();
        }

        var addedOrderItem = await GetTest<OrderItem>(orderItemsUrls[0]);
        addedOrderItem.Should().NotBeNull();

        if (addedOrderItem != null)
        {
            var orderItem = new TestOrderItemUpdate();
            orderItem.Quantity = addedOrderItem.Quantity + 999;
            await PatchTest(orderItemsUrls[0], orderItem);
            updatedOrder = await GetTest<Order>(order.Item2);
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
        var order = AddOrder();

        int numberOfOrderItems = 10;

        var random = new Random();
        string[] orderItemsUrls = new string[numberOfOrderItems];
        for (var i = 0; i < numberOfOrderItems; ++i)
        {
            var orderItem = new TestOrderItem();
            orderItem.Quantity = random.Next(1, 1000);
            orderItem.OrderId = order.Item1;
            orderItem.UnitPrice = new decimal(random.NextDouble() + 1.0);
            orderItem.ExchangeRateToPayOutCurrency = new decimal(random.NextDouble() + 1.0);
            var orderItemUrl = await PostTest(UrlOrderItems, orderItem);
            orderItemsUrls[i] = orderItemUrl;
        }

        async Task CompareTotals()
        {
            decimal total = new decimal(0);
            foreach (var url in orderItemsUrls)
            {
                var orderItem = await GetTest<OrderItem>(url);
                orderItem.Should().NotBeNull();
                if (orderItem != null)
                {
                    total += orderItem.Total;
                }                
            }

            var updatedOrder = await GetTest<Order>(order.Item2);
            updatedOrder.Should().NotBeNull();
            if (updatedOrder != null)
            {
                updatedOrder.Total.Should().Be(total);
            }
        }

        await CompareTotals();

        var addedOrderItem = await GetTest<OrderItem>(orderItemsUrls[0]);
        addedOrderItem.Should().NotBeNull();
        var updatedOrderItem = new TestOrderItemUpdate();
        if (addedOrderItem != null)
        {
            updatedOrderItem.UnitPrice = addedOrderItem.UnitPrice;
            updatedOrderItem.Quantity = addedOrderItem.Quantity + 999;
        }

        await PatchTest(orderItemsUrls[0], updatedOrderItem);
        await CompareTotals();

        addedOrderItem = await GetTest<OrderItem>(orderItemsUrls[0]);
        addedOrderItem.Should().NotBeNull();
        updatedOrderItem = new TestOrderItemUpdate();
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
        var order = AddOrder();

        var random = new Random();
        var orderItem = new TestOrderItem();
        orderItem.Quantity = random.Next(1, 1000);
        orderItem.OrderId = order.Item1;
        orderItem.UnitPrice = new decimal(random.NextDouble() + 1.0);
        orderItem.ExchangeRateToPayOutCurrency = new decimal(random.NextDouble() + 1.0);
        var orderItemUrl = await PostTest(UrlOrderItems, orderItem);

        async Task CompareItems()
        {
            var addedOrderItem = await GetTest<OrderItem>(orderItemUrl);
            addedOrderItem.Should().NotBeNull();

            if (addedOrderItem != null)
            {
                addedOrderItem.CurrencyTotal.Should().Be(orderItem.Quantity * orderItem.UnitPrice);
                addedOrderItem.Total.Should().Be(addedOrderItem.CurrencyTotal * orderItem.ExchangeRateToPayOutCurrency);
            }
        }

        await CompareItems();

        var addedOrderItem = await GetTest<OrderItem>(orderItemUrl);
        addedOrderItem.Should().NotBeNull();
        var updatedOrderItem = new TestOrderItemUpdate();
        if (addedOrderItem != null)
        {
            updatedOrderItem.Quantity = addedOrderItem.Quantity + 10;
            updatedOrderItem.UnitPrice = addedOrderItem.UnitPrice;
        }

        await PatchTest(orderItemUrl, orderItem);
        await CompareItems();

        addedOrderItem = await GetTest<OrderItem>(orderItemUrl);
        addedOrderItem.Should().NotBeNull();
        updatedOrderItem = new TestOrderItemUpdate();
        if (addedOrderItem != null)
        {
            updatedOrderItem.Quantity = addedOrderItem.Quantity;
            updatedOrderItem.UnitPrice = addedOrderItem.UnitPrice + new decimal(1.0);
        }

        await PatchTest(orderItemUrl, orderItem);
        await CompareItems();
    }

    private string AddOrderItem(int orderId, int quantity)
    {
        var orderItem = new TestOrderItem();
        orderItem.Quantity = quantity;
        orderItem.OrderId = orderId;
        var orderItemUrl = PostTest(UrlOrderItems, orderItem).Result;
        return orderItemUrl;
    }

    private (int, string) AddOrder()
    {
        var testOrder = AddCustomerAndCreateOrder();

        var newOrderUrl = PostTest(UrlOrders, testOrder).Result;

        var order = GetTest<Order>(newOrderUrl).Result;

        order.Should().NotBeNull();

        return (order != null ? order.Id : 0, newOrderUrl);
    }

    private (int, string) AddCustomer()
    {
        var testCustomer = new CustomerCreateDto();

        testCustomer.Email = "testEmail@gmail.com";

        var customerUrl = PostTest("/api/customers", testCustomer).Result;

        var customer = GetTest<Customer>(customerUrl).Result;

        customer.Should().NotBeNull();

        return (customer == null ? 0 : customer.Id, customerUrl);
    }

    private OrderCreateDto AddCustomerAndCreateOrder()
    {
        var addedCustomer = AddCustomer();

        var customerId = addedCustomer.Item1;

        var testOrder = new TestOrder();
        testOrder.CustomerId = customerId;

        return testOrder;
    }
}
