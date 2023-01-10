// <copyright file="SimpleTableTests.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Text.Json;
using FluentAssertions;
using OnlineSales.Entities;
using OnlineSales.Infrastructure;

namespace OnlineSales.Tests;

public abstract class SimpleTableTests<T, TC, TU> : BaseTest
    where T : BaseEntity
    where TC : class
    where TU : new()
{
    protected readonly string itemsUrl;
    protected readonly string itemsUrlNotFound;

    protected SimpleTableTests(string url)
    {
        itemsUrl = url;
        itemsUrlNotFound = url + "/404";
    }

    [Fact]
    public async Task GetAllTest()
    {
        await GetAllWithAuthentification();
    }

    [Fact]
    public async Task GetItemNotFoundTest()
    {
        await GetTest(itemsUrlNotFound, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateAndGetItemTest()
    {
        await CreateAndGetItemWithAuthentification();
    }

    [Fact]
    public virtual async Task UpdateItemNotFoundTest()
    {
        await PatchTest(itemsUrlNotFound, new TU(), HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateAndUpdateItemTest()
    {
        var testCreateItem = await CreateItem();

        var testUpdateItem = UpdateItem(testCreateItem.Item1);

        await PatchTest(testCreateItem.Item2, testUpdateItem!);

        var item = await GetTest<T>(testCreateItem.Item2);

        item.Should().BeEquivalentTo(testCreateItem.Item1);
    }

    [Fact]
    public async Task DeleteItemNotFoundTest()
    {
        await DeleteTest(itemsUrlNotFound, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateAndDeleteItemTest()
    {
        var testCreateItem = await CreateItem();

        await DeleteTest(testCreateItem.Item2);

        await GetTest(testCreateItem.Item2, HttpStatusCode.NotFound);
    }

    [Theory]
    [InlineData(true, "", 1, 1)]
    [InlineData(true, "filter[where][id][eq]=1", 1, 1)]
    [InlineData(true, "filter[where][id][eq]=100", 0, 0)]
    [InlineData(true, "filter[limit]=10&filter[skip]=0", 1, 1)]
    [InlineData(true, "filter[limit]=10&filter[skip]=100", 1, 0)]
    [InlineData(false, "", 0, 0)]
    [InlineData(false, "filter[where][id][eq]=1", 0, 0)]
    public async Task GetTotalCountTest(bool createTestItem, string filter, int totalCount, int payloadItemsCount)
    {
        if (createTestItem)
        {
            await CreateItem();
        }

        var response = await GetTest($"{this.itemsUrl}?{filter}");
        response.Should().NotBeNull();

        var totalCountHeader = response.Headers.GetValues(ResponseHeaderNames.TotalCount).FirstOrDefault();
        totalCountHeader.Should().BeEquivalentTo($"{totalCount}");
        var content = await response.Content.ReadAsStringAsync();
        var payload = DeserializePayload<List<T>>(content);
        payload.Should().NotBeNull();
        payload.Should().HaveCount(payloadItemsCount);
    }

    [Theory]
    [InlineData("", 150, 70)]
    [InlineData("filter[skip]=0", 150, 70)]
    [InlineData("filter[limit]=10&filter[skip]=0", 150, 70)]
    public async Task LimitLists(string filter, int dataCount, int limitPerRequest)
    {
        GenerateBulkRecords(dataCount);

        var response = await GetTest($"{this.itemsUrl}?{filter}");
        response.Should().NotBeNull();

        var json = await response.Content.ReadAsStringAsync();

        var deserialized = JsonSerializer.Deserialize<List<T>>(json!);

        int returendCount = deserialized!.Count!;

        Assert.True(returendCount <= limitPerRequest);
    }

    [Theory]
    [InlineData("filter[limit]=550", 150)]
    public async Task InvalidLimit(string filter, int dataCount)
    {
        GenerateBulkRecords(dataCount);

        await GetTest($"{this.itemsUrl}?{filter}", HttpStatusCode.UnprocessableEntity);
    }

    protected virtual async Task<(TC, string)> CreateItem()
    {
        var testCreateItem = TestData.Generate<TC>();

        var newItemUrl = await PostTest(itemsUrl, testCreateItem);

        return (testCreateItem, newItemUrl);
    }

    protected virtual void GenerateBulkRecords(int dataCount, Action<TC>? populateAttributes = null)
    {
        var bulkList = TestData.GenerateAndPopulateAttributes<TC>(dataCount, populateAttributes);
        var bulkEntitiesList = mapper.Map<List<T>>(bulkList);

        App.PopulateBulkData(bulkEntitiesList);
    }

    protected async Task GetAllWithAuthentification(string getAuthToken = "Success")
    {
        const int numberOfItems = 10;
        GenerateBulkRecords(numberOfItems);

        var items = await GetTest<List<T>>(itemsUrl, HttpStatusCode.OK, getAuthToken);

        items.Should().NotBeNull();
        items!.Count.Should().Be(numberOfItems);
    }

    protected async Task CreateAndGetItemWithAuthentification(string getAuthToken = "Success")
    {
        var testCreateItem = await CreateItem();

        var item = await GetTest<T>(testCreateItem.Item2, HttpStatusCode.OK, getAuthToken);

        item.Should().BeEquivalentTo(testCreateItem.Item1);
    }

    protected abstract TU UpdateItem(TC createdItem);
}