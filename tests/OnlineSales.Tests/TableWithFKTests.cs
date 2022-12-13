// <copyright file="TableWithFKTests.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using FluentAssertions;
using Namotion.Reflection;
using Nest;
using OnlineSales.DTOs;
using OnlineSales.Entities;

namespace OnlineSales.Tests;

public abstract class TableWithFKTests<T, TC, TU, TFK, TFKC> : SimpleTableTests<T, TC, TU>
    where T : BaseEntity
    where TC : new()
    where TU : new()
    where TFK : BaseEntity
    where TFKC : new()
{
    private readonly string fkItemsUrl;

    protected TableWithFKTests(string url, string fkItemsUrl)
        : base(url)
    {
        this.fkItemsUrl = fkItemsUrl;
    }

    [Fact]
    public async Task CreateItemWithNonExistedFKItemTest()
    {
        var testItem = new TC();
        await PostTest(itemsUrl, testItem, HttpStatusCode.InternalServerError);
    }

    [Fact]

    public async Task UpdateItemWithNonExistedFKItemTest()
    {
        var testCreateItem = await CreateItem();

        var testUpdateItem = UpdateItem(testCreateItem.Item1);

        ChangeFKId(testUpdateItem, 999);

        await PatchTest(testCreateItem.Item2, testUpdateItem!, HttpStatusCode.InternalServerError);
    }

    [Fact]

    public async Task CascadeDeleteTest()
    {
        var fkItem = await CreateFKItem();

        var fkItemId = fkItem.Item1;

        int numberOfItems = 10;

        string[] itemsUrls = new string[numberOfItems];

        for (var i = 0; i < numberOfItems; ++i)
        {
            var testItem = await CreateItem(fkItemId);

            itemsUrls[i] = testItem.Item2;
        }

        await DeleteTest(fkItem.Item2);

        for (var i = 0; i < numberOfItems; ++i)
        {
            await GetTest<T>(itemsUrls[i], HttpStatusCode.NotFound);
        }             
    }

    protected async Task<(int, string)> CreateFKItem()
    {
        var fkItemCreate = new TFKC();

        var fkUrl = await PostTest(fkItemsUrl, fkItemCreate);

        var fkItem = await GetTest<TFK>(fkUrl);

        fkItem.Should().NotBeNull();

        return (fkItem!.Id, fkUrl);
    }

    protected override async Task<(TC, string)> CreateItem()
    {
        var fkItem = await CreateFKItem();

        var fkId = fkItem.Item1;

        return await CreateItem(fkId);
    }

    protected abstract Task<(TC, string)> CreateItem(int fkId);

    protected abstract void ChangeFKId(TU item, int fkId);
}