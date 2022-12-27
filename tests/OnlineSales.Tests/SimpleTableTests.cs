// <copyright file="SimpleTableTests.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using FluentAssertions;
using OnlineSales.Entities;
using OnlineSales.Infrastructure;

namespace OnlineSales.Tests;

public abstract class SimpleTableTests<T, TC, TU> : BaseTest
    where T : BaseEntity
    where TC : new()
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
    public async Task GetItemNotFoundTest()
    {
        await GetTest(itemsUrlNotFound, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateAndGetItemTest()
    {
        var testCreateItem = await CreateItem();

        var item = await GetTest<T>(testCreateItem.Item2);

        item.Should().BeEquivalentTo(testCreateItem.Item1);
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
    
    [Fact]
    public async Task GetTotalCountTest()
    {
        var testItem = await CreateItem();
        testItem.Should().NotBeNull();

        var response0 = await GetTest(this.itemsUrl);
        response0.Should().NotBeNull();
        var totalCountHeader0 = response0.Headers.GetValues(ResponseHeaderNames.TotalCount).FirstOrDefault();
        totalCountHeader0.Should().BeEquivalentTo("1");
        var content0 = await response0.Content.ReadAsStringAsync();
        var payload0 = DeserializePayload<List<T>>(content0);
        payload0.Should().NotBeNull();
        payload0.Should().HaveCount(1);

        var response1 = await GetTest($"{this.itemsUrl}?filter[where][id][eq]=1");
        response1.Should().NotBeNull();
        var totalCountHeader1 = response1.Headers.GetValues(ResponseHeaderNames.TotalCount).FirstOrDefault();
        totalCountHeader1.Should().BeEquivalentTo("1");
        var content1 = await response1.Content.ReadAsStringAsync();
        var payload1 = DeserializePayload<List<T>>(content1);
        payload1.Should().NotBeNull();
        payload1.Should().HaveCount(1);

        var response2 = await GetTest($"{this.itemsUrl}?filter[where][id][eq]=100");
        var totalCountHeader2 = response2.Headers.GetValues(ResponseHeaderNames.TotalCount).FirstOrDefault();
        totalCountHeader2.Should().BeEquivalentTo("0");
        var content2 = await response2.Content.ReadAsStringAsync();
        var payload2 = DeserializePayload<List<T>>(content2);
        payload2.Should().NotBeNull();
        payload2.Should().HaveCount(0);

        var response3 = await GetTest($"{this.itemsUrl}?filter[limit]=10&filter[skip]=0");
        var totalCountHeader3 = response3.Headers.GetValues(ResponseHeaderNames.TotalCount).FirstOrDefault();
        totalCountHeader3.Should().BeEquivalentTo("1");
        var content3 = await response3.Content.ReadAsStringAsync();
        var payload3 = DeserializePayload<List<T>>(content3);
        payload3.Should().NotBeNull();
        payload3.Should().HaveCount(1);

        var response4 = await GetTest($"{this.itemsUrl}?filter[limit]=10&filter[skip]=100");
        var totalCountHeader4 = response4.Headers.GetValues(ResponseHeaderNames.TotalCount).FirstOrDefault();
        totalCountHeader4.Should().BeEquivalentTo("1");
        var content4 = await response4.Content.ReadAsStringAsync();
        var payload4 = DeserializePayload<List<T>>(content4);
        payload4.Should().NotBeNull();
        payload4.Should().HaveCount(0);

        await DeleteTest(testItem.Item2);

        var response5 = await GetTest($"{this.itemsUrl}?filter[where][id][eq]=1");
        response5.Should().NotBeNull();
        var totalCountHeader5 = response5.Headers.GetValues(ResponseHeaderNames.TotalCount).FirstOrDefault();
        totalCountHeader5.Should().BeEquivalentTo("0");
        var content5 = await response5.Content.ReadAsStringAsync();
        var payload5 = DeserializePayload<List<T>>(content5);
        payload5.Should().NotBeNull();
        payload5.Should().HaveCount(0);

        var response6 = await GetTest(this.itemsUrl);
        response6.Should().NotBeNull();
        var totalCountHeader6 = response6.Headers.GetValues(ResponseHeaderNames.TotalCount).FirstOrDefault();
        totalCountHeader6.Should().BeEquivalentTo("0");
        var content6 = await response6.Content.ReadAsStringAsync();
        var payload6 = DeserializePayload<List<T>>(content6);
        payload6.Should().NotBeNull();
        payload6.Should().HaveCount(0);
    }

    protected virtual async Task<(TC, string)> CreateItem()
    {
        var testCreateItem = new TC();

        var newItemUrl = await PostTest(itemsUrl, testCreateItem);

        return (testCreateItem, newItemUrl);
    }

    protected abstract TU UpdateItem(TC createdItem);
}