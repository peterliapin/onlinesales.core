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
    public async Task GetWithLimitTest()
    {
        int limit = 5;
        GenerateBulkRecords(limit);

        var result = await GetTest<List<Order>>(itemsUrl + string.Format("?filter[limit]={0}", limit));

        result.Should().NotBeNull();
        result!.Count.Should().Be(limit);
    }

    [Fact]
    public async Task GetWithOrderByIdTest()
    {
        int numberOfItems = 10;
        GenerateBulkRecords(numberOfItems);

        var result = await GetTest<List<Order>>(itemsUrl + "?filter[order]=Id%20ASC");

        result.Should().NotBeNull();
        result!.Count.Should().Be(numberOfItems);

        for (int i = 0; i < numberOfItems; ++i)
        {
            result[i].Id.Should().Be(i + 1);
        }

        result = await GetTest<List<Order>>(itemsUrl + "?filter[order]=Id%20DESC");

        result.Should().NotBeNull();
        result!.Count.Should().Be(numberOfItems);

        for (int i = 0; i < numberOfItems; ++i)
        {
            result[i].Id.Should().Be(numberOfItems - i);
        }
    }

    [Fact]
    public async Task GetWithOrderByTwoPropertiesTest()
    {
        var index = 0;
        int numberOfItems = 10;

        var populateAttributes = (TestOrder order) =>
        {            
            order.AffiliateName = (index / 2).ToString();
            index++;
        };

        GenerateBulkRecords(numberOfItems, populateAttributes);

        var result = await GetTest<List<Order>>(itemsUrl + "?filter[order][0]=AffiliateName%20ASC&filter[order][1]=Id%20DESC");

        result.Should().NotBeNull();
        result!.Count.Should().Be(numberOfItems);

        for (int i = 0; i < numberOfItems; ++i)
        {
            result[i].AffiliateName.Should().Be((i / 2).ToString());
            if (i + 1 < numberOfItems && result[i].AffiliateName == result[i + 1].AffiliateName)
            {
                result[i + 1].Id.Should().BeLessThan(result[i].Id);
            }
        }
    }

    [Fact]
    public async Task GetWithSkipTest()
    {
        int numberOfItems = 30;
        GenerateBulkRecords(numberOfItems);

        async Task GetAndCheck(int skipItemsNumber)
        {
            var result = await GetTest<List<Order>>(itemsUrl + string.Format("?filter[skip]={0}", skipItemsNumber));
            result.Should().NotBeNull();
            result!.Count.Should().Be(numberOfItems >= skipItemsNumber ? numberOfItems - skipItemsNumber : 0);
        }

        await GetAndCheck(0 * numberOfItems);

        await GetAndCheck((int)(0.25 * numberOfItems));

        await GetAndCheck((int)(0.5 * numberOfItems));

        await GetAndCheck(numberOfItems);

        await GetAndCheck((int)(1.5 * numberOfItems));
    }

    [Fact]
    public async Task GetWithWhereTest()
    {
        var index = 0;
        int numberOfItems = 10;

        var populateAttributes = (TestOrder order) =>
        {
            order.AffiliateName = ((index + 1) % 2 == 0) ? "even" : "odd";
            index++;
        };

        GenerateBulkRecords(numberOfItems, populateAttributes);

        var result = await GetTest<List<Order>>(itemsUrl + string.Format("?filter[where][Id]={0}", numberOfItems / 2));
        result.Should().NotBeNull();
        result!.Count.Should().Be(1);
        result[0].Id.Should().Be(numberOfItems / 2);

        result = await GetTest<List<Order>>(itemsUrl + string.Format("?filter[where][Id][neq]={0}", numberOfItems / 2));
        result.Should().NotBeNull();
        result!.Count.Should().Be(numberOfItems - 1);
                
        result = await GetTest<List<Order>>(itemsUrl + string.Format("?filter[where][Id][gte]={0}", numberOfItems / 2));
        result.Should().NotBeNull();
        result!.Count.Should().Be(1 + (numberOfItems / 2));

        result = await GetTest<List<Order>>(itemsUrl + string.Format("?filter[where][Id][lte]={0}", numberOfItems / 2));
        result.Should().NotBeNull();
        result!.Count.Should().Be(numberOfItems / 2);

        result = await GetTest<List<Order>>(itemsUrl + "?filter[where][or][Id][lte]=2&filter[where][or][Id][gte]=9");
        result.Should().NotBeNull();
        result!.Count.Should().Be(4);        
    }

    [Fact]

    public async Task GetWithIncorrectQueryTest()
    {
        GenerateBulkRecords(10);

        await GetTest<List<Order>>(itemsUrl + "?SomeIncorrectQuery", HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetWithPagingTest()
    {
        const int numberOfItems = 100;
        const int pageSize = 10;

        GenerateBulkRecords(numberOfItems);

        async Task GetAndCheck(int skipItemsNumber, int expectedItemsNumber = pageSize)
        {
            var result = await GetTest<List<Order>>(itemsUrl + string.Format("?filter[where][Id][lte]={0}&filter[limit]={1}&filter[skip]={2}", numberOfItems / 2, pageSize, skipItemsNumber));
            result.Should().NotBeNull();
            result!.Count.Should().Be(expectedItemsNumber);
            for (int i = 0; i < expectedItemsNumber; ++i)
            {
                result[i].Id.Should().Be(skipItemsNumber + i + 1);
            }
        }

        await GetAndCheck(0);
        await GetAndCheck(10);
        await GetAndCheck(20);
        await GetAndCheck(30);
        await GetAndCheck(40);
        await GetAndCheck(50, 0);
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

    [Fact]
    public async Task ImportFileWithoutContactId()
    {
        await CreateFKItem();
        await PostImportTest(itemsUrl, "ordersNoFK.csv");

        var addedOrder = App.GetDbContext() !.Orders!.First(o => o.Id == 1);
        addedOrder.Should().NotBeNull();
        addedOrder.ContactId.Should().Be(1);
    }

    protected override async Task<(TestOrder, string)> CreateItem(string uid, int fkId)
    {
        var testOrder = new TestOrder(uid, fkId);

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
        var fkItemCreate = new TestContact();

        var fkUrl = await PostTest("/api/contacts", fkItemCreate);

        var fkItem = await GetTest<Contact>(fkUrl);

        fkItem.Should().NotBeNull();

        return (fkItem!.Id, fkUrl);
    }
}