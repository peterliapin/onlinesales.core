// <copyright file="OrdersTests.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using OnlineSales.DataAnnotations;
using OnlineSales.Infrastructure;

namespace OnlineSales.Tests;

public class OrdersTests : TableWithFKTests<Order, TestOrder, OrderUpdateDto>
{
    public OrdersTests()
        : base("/api/orders")
    {
    }

    [Fact]
    public async Task GetWithWhereLikeTest()
    {
        var fkItem = CreateFKItem().Result;
        var fkId = fkItem.Item1;

        var bulkEntitiesList = new List<Order>();

        var bulkList = TestData.GenerateAndPopulateAttributes<TestOrder>("1", tc => tc.AffiliateName = "1 Test q", fkId);
        bulkEntitiesList.Add(mapper.Map<Order>(bulkList));
        bulkList = TestData.GenerateAndPopulateAttributes<TestOrder>("2", tc => tc.AffiliateName = "Test 2 z q", fkId);
        bulkEntitiesList.Add(mapper.Map<Order>(bulkList));
        bulkList = TestData.GenerateAndPopulateAttributes<TestOrder>("3", tc => tc.AffiliateName = "Test 3 q", fkId);
        bulkEntitiesList.Add(mapper.Map<Order>(bulkList));
        bulkList = TestData.GenerateAndPopulateAttributes<TestOrder>("4", tc => tc.AffiliateName = "Te1st 4 q", fkId);
        bulkEntitiesList.Add(mapper.Map<Order>(bulkList));

        App.PopulateBulkData(bulkEntitiesList);

        await SyncElasticSearch();

        var result = await GetTest<List<Order>>(itemsUrl + "?filter[where][AffiliateName][like]=.*est&query=q");
        result!.Count.Should().Be(3);
    }

    [Fact]
    public async Task GetWithIncludeTest()
    {
        int numberOfOrderItems = 10;
        string affName = "Aff";

        var fkItem = CreateFKItem().Result;
        var fkId = fkItem.Item1;
        App.PopulateBulkData(mapper.Map<Order>(TestData.GenerateAndPopulateAttributes<TestOrder>("1", o => o.AffiliateName = affName, fkId)));
        App.PopulateBulkData(mapper.Map<List<OrderItem>>(TestData.GenerateAndPopulateAttributes<TestOrderItem>(numberOfOrderItems, null, 1)));
        await SyncElasticSearch();

        var orderWithContactAndItems = await GetTest<List<Order>>(itemsUrl + $"?query={affName}&filter[include]=Contact&filter[include]=OrderItems&filter[where][Id]=1");
        orderWithContactAndItems!.Count.Should().Be(1);
        orderWithContactAndItems[0].Contact!.Should().NotBeNull();
        orderWithContactAndItems[0].OrderItems.Should().NotBeNull();
        orderWithContactAndItems[0].OrderItems!.Count.Should().Be(numberOfOrderItems);
        foreach (var orderItem in orderWithContactAndItems[0].OrderItems!)
        {
            orderItem.Should().NotBeNull();
        }

        var orderItemsWithOrder = await GetTest<List<OrderItem>>($"/api/order-items?query=USD&filter[include]=Order&filter[where][Id][gt]={numberOfOrderItems / 2}");
        orderItemsWithOrder.Should().NotBeNull();
        orderItemsWithOrder!.Count.Should().Be(numberOfOrderItems / 2);
        foreach (var orderItem in orderItemsWithOrder)
        {
            orderItem.Should().NotBeNull();
            orderItem.Order.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task GetWithSearchTest()
    {
        var fkItem = CreateFKItem().Result;
        var fkId = fkItem.Item1;

        var bulkEntitiesList = new List<Order>();

        var bulkList = TestData.GenerateAndPopulateAttributes<TestOrder>("1", null, fkId);
        bulkEntitiesList.Add(mapper.Map<Order>(bulkList));
        string testAN = "Nearly ten years had passed since the Dursleys had woken up to find their nephew on the front step" +
        ", but Privet Drive had hardly changed at all. The sun rose on the same tidy front gardens and lit up the brass number four on the Dursleys' front door;" +
        " it crept into their living room, which was almost exactly the same as it had been on the night when Mr. Dursley had seen that fateful news report about the owls. ";
        bulkList = TestData.GenerateAndPopulateAttributes<TestOrder>("2", tc => tc.AffiliateName = testAN, fkId);        
        bulkEntitiesList.Add(mapper.Map<Order>(bulkList));
        bulkList = TestData.GenerateAndPopulateAttributes<TestOrder>("3", tc => tc.ExchangeRate = 123.456M, fkId);
        bulkEntitiesList.Add(mapper.Map<Order>(bulkList));
        bulkList = TestData.GenerateAndPopulateAttributes<TestOrder>("4", tc => tc.AffiliateName = testAN, fkId);
        bulkEntitiesList.Add(mapper.Map<Order>(bulkList));
        App.PopulateBulkData(bulkEntitiesList);
        await SyncElasticSearch();

        var result = await GetTest<List<Order>>(itemsUrl + "?query=fatefully&filter[order]=Id");
        result!.Count.Should().Be(2);
        result[0].AffiliateName.Should().Be(testAN);

        result = await GetTest<List<Order>>(itemsUrl + "?query=fatefully&filter[order]=ContactIp");
        result!.Count.Should().Be(2);
        result[0].AffiliateName.Should().Be(testAN);

        result = await GetTest<List<Order>>(itemsUrl + "?query=fatefully");
        result!.Count.Should().Be(2);
        result[0].AffiliateName.Should().Be(testAN);

        result = await GetTest<List<Order>>(itemsUrl + "?query=123.456");
        result!.Count.Should().Be(1);
        result[0].ExchangeRate.Should().Be(123.456M);

        result = await GetTest<List<Order>>(itemsUrl + "?query=");
        result!.Count.Should().Be(4);

        result = await GetTest<List<Order>>(itemsUrl + "?query=SomeSearchString");
        result!.Count.Should().Be(0);
    }

    [Fact]
    public async Task GetWithSearchWithBigLimitTest()
    {
        int entitiesNumber = 13000;
        var fkItem = CreateFKItem().Result;
        var fkId = fkItem.Item1;

        var bulkList = TestData.GenerateAndPopulateAttributes<TestOrder>(entitiesNumber, to => to.AffiliateName = "AffiliateName", fkId);
        var bulkEntitiesList = mapper.Map<List<Order>>(bulkList);

        App.PopulateBulkData(bulkEntitiesList);

        string totalCountHeader = string.Empty;
        while (totalCountHeader != $"{entitiesNumber}")
        {
            await SyncElasticSearch();
            var resp = await GetTest(itemsUrl + "?query=AffiliateName");
            totalCountHeader = resp.Headers.GetValues(ResponseHeaderNames.TotalCount).FirstOrDefault() !;
        }

        async Task TestSkipAndLimit(int skip, int limit, int expextedSize)
        {
            var response = await GetTestCSV<OrderImportDto>(itemsUrl + $"/export/?query=AffiliateName&filter[skip]={skip}&filter[limit]={limit}");
            response.Should().NotBeNull();
            response!.Count.Should().Be(expextedSize);
            for (int i = 0; i < response.Count; ++i)
            {
                response[i].Id.Should().Be(skip + 1 + i);
            }
        }

        await TestSkipAndLimit(0, 14000, entitiesNumber);        
        await TestSkipAndLimit(5000, 1000, 1000);
        await TestSkipAndLimit(11000, 2000, 2000);
        await TestSkipAndLimit(12000, 2000, 1000);
        await TestSkipAndLimit(14000, 2000, 0);
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
            order.Currency = "Test";
            index++;            
        };

        GenerateBulkRecords(numberOfItems, populateAttributes);

        var result = await GetTest<List<Order>>(itemsUrl + "?filter[order][0]=AffiliateName%20ASC&filter[order][1]=Id%20DESC&filter[where][Currency]=Test");

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

        var result = await GetTest<List<Order>>(itemsUrl + "?filter[where][or][Id][lte]=2&filter[where][or][Id][gte]=9");
        result.Should().NotBeNull();
        result!.Count.Should().Be(4);

        result = await GetTest<List<Order>>(itemsUrl + string.Format("?filter[where][Id]={0}", numberOfItems / 2));
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
    }

    [Fact]

    public async Task GetWithIncorrectQueryTest()
    {
        GenerateBulkRecords(10);

        await GetTest<List<Order>>(itemsUrl + "?incorrect-query", HttpStatusCode.BadRequest);
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