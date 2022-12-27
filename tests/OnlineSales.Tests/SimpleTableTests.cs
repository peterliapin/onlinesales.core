// <copyright file="SimpleTableTests.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using FluentAssertions;
using OnlineSales.DTOs;
using OnlineSales.Entities;

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
    public async Task GetAllTest()
    {
        await GetAllTestImpl();
    }

    [Fact]
    public async Task GetItemNotFoundTest()
    {
        await GetTest(itemsUrlNotFound, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateAndGetItemTest()
    {
        await CreateAndGetItemTestImpl();
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

    protected virtual async Task<(TC, string)> CreateItem()
    {
        var testCreateItem = new TC();

        var newItemUrl = await PostTest(itemsUrl, testCreateItem);

        return (testCreateItem, newItemUrl);
    }

    protected async Task GetAllTestImpl(string getAuthToken = "Success")
    {
        const int itemsNumber = 10;

        for (int i = 0; i < itemsNumber; ++i)
        {
            await CreateItem();
        }

        var items = await GetTest<List<T>>(itemsUrl, HttpStatusCode.OK, getAuthToken);

        items.Should().NotBeNull();
        items!.Count.Should().Be(itemsNumber);
    }

    protected async Task CreateAndGetItemTestImpl(string getAuthToken = "Success")
    {
        var testCreateItem = await CreateItem();

        var item = await GetTest<T>(testCreateItem.Item2, HttpStatusCode.OK, getAuthToken);

        item.Should().BeEquivalentTo(testCreateItem.Item1);
    }

    protected abstract TU UpdateItem(TC createdItem);
}