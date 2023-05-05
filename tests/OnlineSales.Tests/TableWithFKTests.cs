// <copyright file="TableWithFKTests.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

namespace OnlineSales.Tests;

public abstract class TableWithFKTests<T, TC, TU, TS> : SimpleTableTests<T, TC, TU, TS>
    where T : BaseEntity
    where TC : class
    where TU : new()
    where TS : ISaveService<T>
{
    protected TableWithFKTests(string url)
        : base(url)
    {
    }

    [Fact]
    public virtual async Task CreateItemWithNonExistedFKItemTest()
    {
        var testItem = TestData.Generate<TC>(string.Empty, 0);
        await this.PostTest(this.itemsUrl, testItem, HttpStatusCode.UnprocessableEntity);
    }

    [Fact]

    public async Task CascadeDeleteTest()
    {
        var fkItem = await this.CreateFKItem();

        var fkItemId = fkItem.Item1;

        var numberOfItems = 10;

        var itemsUrls = new string[numberOfItems];

        for (var i = 0; i < numberOfItems; ++i)
        {
            var testItem = await this.CreateItem(i.ToString(), fkItemId);

            itemsUrls[i] = testItem.Item2;
        }

        await this.DeleteTest(fkItem.Item2);

        for (var i = 0; i < numberOfItems; ++i)
        {
            await this.GetTest<T>(itemsUrls[i], HttpStatusCode.NotFound);
        }
    }

    protected abstract Task<(int, string)> CreateFKItem(string authToken = "Success");

    protected override async Task<(TC, string)> CreateItem(string authToken = "Success")
    {
        var fkItem = await this.CreateFKItem();

        var fkId = fkItem.Item1;

        return await this.CreateItem(string.Empty, fkId);
    }

    protected override void GenerateBulkRecords(int dataCount, Action<TC>? populateAttributes = null)
    {
        var fkItem = this.CreateFKItem().Result;
        var fkId = fkItem.Item1;

        var bulkList = TestData.GenerateAndPopulateAttributes<TC>(dataCount, populateAttributes, fkId);
        var bulkEntitiesList = this.mapper.Map<List<T>>(bulkList);

        App.PopulateBulkData<T, TS>(bulkEntitiesList);
    }

    protected abstract Task<(TC, string)> CreateItem(string uid, int fkId);
}