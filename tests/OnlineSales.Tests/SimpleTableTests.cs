// <copyright file="SimpleTableTests.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Text.Json;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSales.Helpers;
using OnlineSales.Infrastructure;

namespace OnlineSales.Tests;

public abstract class SimpleTableTests<T, TC, TU, TS> : BaseTest
    where T : BaseEntityWithId
    where TC : class
    where TU : new()
    where TS : ISaveService<T>
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
        var testCreateItem = await CreateItem();

        var testUpdateItem = UpdateItem(testCreateItem.Item1);

        await PatchTest(itemsUrlNotFound, testUpdateItem!, HttpStatusCode.NotFound);
    }

    [Fact]
    public virtual async Task CreateAndCheckEntityState_ChangeLog()
    {
        var testCreateItem = await CreateItem();

        var item = await GetTest<T>(testCreateItem.Item2);

        var result = App.GetDbContext()!.ChangeLogs!.FirstOrDefault(c => c.ObjectId == item!.Id && c.ObjectType == typeof(T).Name && c.EntityState == EntityState.Added)!;

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateAndUpdateItemTest()
    {
        var createAndUpdateItems = await CreateAndUpdateItem();

        var item = await GetTest<T>(createAndUpdateItems.testCreateItem.Item2);

        MustBeEquivalent(createAndUpdateItems.testCreateItem.Item1, item);
    }

    [Fact]
    public virtual async Task CreateAndUpdateCheckEntityState_ChangeLog()
    {
        var createAndUpdateItems = await CreateAndUpdateItem();

        var item = await GetTest<T>(createAndUpdateItems.testCreateItem.Item2);

        var result = App.GetDbContext()!.ChangeLogs!.FirstOrDefault(c => c.ObjectId == item!.Id && c.ObjectType == typeof(T).Name && c.EntityState == EntityState.Modified)!;

        result.Should().NotBeNull();
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
    public async Task CreateAndDeleteCheckEntityState_ChangeLog()
    {
        var testCreateItem = await CreateItem();

        var item = await GetTest<T>(testCreateItem.Item2);

        await DeleteTest(testCreateItem.Item2);

        var result = App.GetDbContext()!.ChangeLogs!.FirstOrDefault(c => c.ObjectId == item!.Id && c.ObjectType == typeof(T).Name && c.EntityState == EntityState.Deleted)!;

        result.Should().NotBeNull();
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

        var response = await GetTest($"{itemsUrl}?{filter}");
        response.Should().NotBeNull();

        var totalCountHeader = response.Headers.GetValues(ResponseHeaderNames.TotalCount).FirstOrDefault();
        totalCountHeader.Should().Be($"{totalCount}");
        var content = await response.Content.ReadAsStringAsync();
        var payload = JsonSerializer.Deserialize<List<T>>(content);
        payload.Should().NotBeNull();
        payload.Should().HaveCount(payloadItemsCount);
    }

    [Theory]
    [InlineData("", 150, 100)]
    [InlineData("filter[skip]=0", 150, 100)]
    [InlineData("filter[limit]=10&filter[skip]=0", 150, 100)]
    public async Task LimitLists(string filter, int dataCount, int limitPerRequest)
    {
        GenerateBulkRecords(dataCount);

        var response = await GetTest($"{itemsUrl}?{filter}");
        response.Should().NotBeNull();

        var json = await response.Content.ReadAsStringAsync();

        var deserialized = JsonSerializer.Deserialize<List<T>>(json!);

        var returendCount = deserialized!.Count!;

        Assert.True(returendCount <= limitPerRequest);
    }

    [Theory]
    [InlineData("filter[limit]=15001", 150)]
    public async Task InvalidLimit(string filter, int dataCount)
    {
        GenerateBulkRecords(dataCount);

        await GetTest($"{itemsUrl}?{filter}", HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData("filtercadabra", HttpStatusCode.BadRequest)]
    [InlineData("filter", HttpStatusCode.BadRequest)]
    [InlineData("filter[]", HttpStatusCode.BadRequest)]
    [InlineData("filter[]=", HttpStatusCode.BadRequest)]
    [InlineData("filter[]=0", HttpStatusCode.BadRequest)]
    [InlineData("filter[][]=3", HttpStatusCode.BadRequest)]
    [InlineData("filter[][][]=4", HttpStatusCode.BadRequest)]
    [InlineData("filter[notexists]=5", HttpStatusCode.BadRequest)]
    [InlineData("filter[where][notexists]=6", HttpStatusCode.BadRequest)]
    [InlineData("filter[where][][]=7", HttpStatusCode.BadRequest)]
    [InlineData("filter[where][id][]=8", HttpStatusCode.BadRequest)]
    [InlineData("filter[where][id][notexists]=9", HttpStatusCode.BadRequest)]
    [InlineData("filter[^7@5\\nwhere][id^7@5\\n][|^7@5\\n]=^7@5\\n", HttpStatusCode.BadRequest)]
    [InlineData("filter[where][id]=^7@5\\n", HttpStatusCode.BadRequest)]
    [InlineData("filter[][id]=^7@5\\n", HttpStatusCode.BadRequest)]
    [InlineData("filter[where][id][eq]=^7@5\\n", HttpStatusCode.BadRequest)]
    [InlineData("filter[limit]=abc", HttpStatusCode.BadRequest)]
    [InlineData("filter[skip]=abc", HttpStatusCode.BadRequest)]
    [InlineData("filter[order]=5555incorrectfield777", HttpStatusCode.BadRequest)]
    public async Task InvalidQueryParameter(string filter, HttpStatusCode code)
    {
        await GetTest($"{itemsUrl}?{filter}", code);
    }

    [Theory]
    [InlineData("filter[where][id]=9", HttpStatusCode.OK)]
    [InlineData("filter[where][id][eq]=9", HttpStatusCode.OK)]
    [InlineData("filter[order]=id", HttpStatusCode.OK)]
    [InlineData("filter[skip]=5", HttpStatusCode.OK)]
    [InlineData("filter[limit]=5", HttpStatusCode.OK)]
    public async Task ValidQueryParameter(string filter, HttpStatusCode code)
    {
        await GetTest($"{itemsUrl}?{filter}", code);
    }

    [Fact]
    public async Task ValidWherePropertyType()
    {
        var query = string.Empty;
        var typeProperties = typeof(T).GetProperties();
        foreach (var property in typeProperties)
        {
            if (!property.PropertyType.IsValueType || (Nullable.GetUnderlyingType(property.PropertyType) != null))
            {
                continue;
            }

            object? defValue;
            if (property.PropertyType == typeof(string))
            {
                defValue = "abc";
            }
            else if (property.PropertyType == typeof(DateTime))
            {
                defValue = DateTime.MinValue.ToString("yyyy-MM-dd'T'HH:mm:ss.fffK");
            }
            else
            {
                defValue = Activator.CreateInstance(property.PropertyType);
            }

            query += HttpUtility.UrlEncode($"filter[where][{property.Name}][eq]={defValue}") + "&";
        }

        query = query.Substring(0, query.Length - 1); // Remove latest '&'
        await GetTest($"{itemsUrl}?{query}", HttpStatusCode.OK);
    }

    [Fact]
    public async Task InvalidWherePropertyType()
    {
        var baseTypesList = new Type[]
        {
            typeof(string),
            typeof(DateTime),
            typeof(int),
        };
        var query = string.Empty;
        var typeProperties = typeof(T).GetProperties();
        foreach (var property in typeProperties)
        {
            if (!property.PropertyType.IsValueType
                || (Nullable.GetUnderlyingType(property.PropertyType) != null)
                || property.PropertyType == typeof(decimal) // Default value for decimal, double, float, long serializes as 0 so skip them
                || property.PropertyType == typeof(double)
                || property.PropertyType == typeof(float)
                || property.PropertyType == typeof(long))
            {
                continue;
            }

            query += baseTypesList.Where(t => t != property.PropertyType).Select(type =>
            {
                object? defValue;
                if (type == typeof(string))
                {
                    defValue = "abc";
                }
                else if (type == typeof(DateTime))
                {
                    defValue = DateTime.MinValue.ToString("yyyy-MM-dd'T'HH:mm:ss.fffK");
                }
                else
                {
                    defValue = Activator.CreateInstance(type);
                }

                return HttpUtility.UrlEncode($"filter[where][{property.Name}][eq]={defValue}") + "&";
            }).Aggregate(string.Empty, (acc, value) => acc + value);
        }

        query = query.Substring(0, query.Length - 1); // Remove latest '&'
        var queryCmds = query.Split('&').Select(s => HttpUtility.UrlDecode(s)).ToList();
        var queryCmdsCount = queryCmds.Count;

        var result = await GetTestRawContentSerialize<ProblemDetails>($"{itemsUrl}?{query}", HttpStatusCode.BadRequest);
        result.Should().NotBeNull();
        var resultDiff = queryCmds.Except(result!.Extensions.Keys).Aggregate(string.Empty, (acc, value) => $"{acc} \n {value}");
        result!.Extensions.Count(pair => pair.Key.ToLowerInvariant() != "traceid").Should().Be(queryCmdsCount, resultDiff);
    }

    protected virtual void MustBeEquivalent(object? expected, object? result)
    {
        result.Should().BeEquivalentTo(expected);
    }

    protected virtual async Task<(TC, string)> CreateItem(string authToken = "Success")
    {
        var testCreateItem = TestData.Generate<TC>();

        var newItemUrl = await PostTest(itemsUrl, testCreateItem, HttpStatusCode.Created, authToken);

        return (testCreateItem, newItemUrl);
    }

    protected virtual void GenerateBulkRecords(int dataCount, Action<TC>? populateAttributes = null)
    {
        var bulkList = TestData.GenerateAndPopulateAttributes<TC>(dataCount, populateAttributes);
        var bulkEntitiesList = mapper.Map<List<T>>(bulkList);

        App.PopulateBulkData<T, TS>(bulkEntitiesList);
    }

    protected async Task GetAllWithAuthentification(string getAuthToken = "Success")
    {
        const int numberOfItems = 10;

        GenerateBulkRecords(numberOfItems);

        var items = await GetTest<List<T>>(itemsUrl, HttpStatusCode.OK, getAuthToken);

        items.Should().NotBeNull();
        items!.Count.Should().Be(numberOfItems);
    }

    protected async Task CreateAndGetItemWithAuthentification(string authToken = "Success")
    {
        var testCreateItem = await CreateItem();

        var item = await GetTest<T>(testCreateItem.Item2, HttpStatusCode.OK, authToken);

        MustBeEquivalent(testCreateItem.Item1, item);
    }

    protected abstract TU UpdateItem(TC createdItem);

    private async Task<((TC, string) testCreateItem, TU? testUpdateItem)> CreateAndUpdateItem()
    {
        var testCreateItem = await CreateItem();

        var testUpdateItem = UpdateItem(testCreateItem.Item1);

        await PatchTest(testCreateItem.Item2, testUpdateItem!);

        return (testCreateItem, testUpdateItem);
    }

    private async Task<TRet?> GetTestRawContentSerialize<TRet>(string url, HttpStatusCode expectedCode = HttpStatusCode.OK, string authToken = "Success")
    where TRet : class
    {
        var response = await GetTest(url, expectedCode, authToken);

        var content = await response.Content.ReadAsStringAsync();

        return JsonHelper.Deserialize<TRet>(content);
    }
}