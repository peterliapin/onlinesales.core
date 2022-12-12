// <copyright file="SimpleTableTests.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using FluentAssertions;
using Namotion.Reflection;
using OnlineSales.DTOs;
using OnlineSales.Entities;

namespace OnlineSales.Tests;

public abstract class SimpleTableTests<T, TC, TU, TUTCConverter> : BaseTest
    where T : BaseEntity
    where TC : new()
    where TU : new()
    where TUTCConverter : ITestTypeConverter<TU, TC>, new()
{
    private readonly string itemsUrl;
    private readonly string itemsUrlNotFound;

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
    public async Task UpdateItemNotFoundTest()
    {
        await PatchTest(itemsUrlNotFound, new { }, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateAndUpdateItemTest()
    {      
        var testCreateItem = await CreateItem();

        var testUpdateItem = new TU();

        testCreateItem.Item1 = new TC();

        var conv = new TUTCConverter();
        conv.Convert(testUpdateItem, testCreateItem.Item1);

        await PatchTest(testCreateItem.Item2, testUpdateItem);

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

    private async Task<(TC, string)> CreateItem()
    {
        var testCreateItem = new TC();

        var newItemUrl = await PostTest(itemsUrl, testCreateItem);

        return (testCreateItem, newItemUrl);
    }

    private async Task CreateAndUpdateItemTestInternal(Func<int, TC> creator)
    {
        var testCreateItem = await CreateItem();

        var testUpdateItem = new TU();

        testCreateItem.Item1 = creator(1);

        await PatchTest(testCreateItem.Item2, testUpdateItem);

        var item = await GetTest<T>(testCreateItem.Item2);

        item.Should().BeEquivalentTo(testCreateItem.Item1);
    }
}