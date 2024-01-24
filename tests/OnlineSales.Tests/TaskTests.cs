// <copyright file="TaskTests.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Xml.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nest;
using OnlineSales.Configuration;
using OnlineSales.Data;
using OnlineSales.Helpers;
using OnlineSales.Interfaces;
using static Azure.Core.HttpHeader;

namespace OnlineSales.Tests;

public class TaskTests : BaseTest
{
    private readonly string tasksUrl = "api/tasks";

    [Fact]
    public async Task GetAllTasksTest()
    {
        var responce = await GetRequest(tasksUrl);

        var content = await responce.Content.ReadAsStringAsync();

        var tasks = JsonHelper.Deserialize<IList<TaskDetailsDto>>(content);

        tasks.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetByNameFailureTest()
    {
        await GetTest(tasksUrl + "/SomeUnexistedTask", HttpStatusCode.NotFound, "Success");
    }

    [Fact]
    public async Task GetByNameSuccesTest()
    {
        var name = "SyncEsTask";

        var responce = await GetTest<TaskDetailsDto>(tasksUrl + "/" + name);

        responce.Should().NotBeNull();
        responce!.Name.Should().Contain("SyncEsTask");
    }

    [Fact]
    public async Task StartAndStopTaskTest()
    {
        var name = "SyncEsTask";

        var responce = await GetTest<TaskDetailsDto>(tasksUrl + "/" + name);
        responce.Should().NotBeNull();
        responce!.IsRunning.Should().BeFalse();

        responce = await GetTest<TaskDetailsDto>(tasksUrl + "/start/" + name);
        responce.Should().NotBeNull();
        responce!.IsRunning.Should().BeTrue();

        responce = await GetTest<TaskDetailsDto>(tasksUrl + "/stop/" + name);
        responce.Should().NotBeNull();
        responce!.IsRunning.Should().BeFalse();
    }

    [Fact]
    public async Task HandleAllChangeLogRecordsTest()
    {
        await CheckIfTaskNotRunning("SyncEsTask");

        var config = App.Services.GetRequiredService<IConfiguration>();
        config.Should().NotBeNull();
        var esSyncBatchSize = config.GetSection("Tasks:SyncEsTask")!.Get<TaskWithBatchConfig>()!.BatchSize;

        App.PopulateBulkData<DealPipeline, IEntityService<DealPipeline>>(mapper.Map<List<DealPipeline>>(TestData.GenerateAndPopulateAttributes<TestDealPipeline>(esSyncBatchSize * 2, null)));

        await SyncElasticSearch();

        CountDocumentsInIndex("onlinesales-dealpipeline").Should().Be(esSyncBatchSize * 2);
    }

    [Fact]
    public async Task ReindexElasticAfterDeletingIndex()
    {
        int dataSize = 10;

        await CheckIfTaskNotRunning("SyncEsTask");

        App.PopulateBulkData<DealPipeline, IEntityService<DealPipeline>>(mapper.Map<List<DealPipeline>>(TestData.GenerateAndPopulateAttributes<TestDealPipeline>(dataSize, null)));

        await SyncElasticSearch();

        CountDocumentsInIndex("onlinesales-dealpipeline").Should().Be(dataSize);

        App.GetElasticClient().Indices.Delete("onlinesales-dealpipeline");

        await SyncElasticSearch();

        CountDocumentsInIndex("onlinesales-dealpipeline").Should().Be(dataSize);
    }

    private async Task CheckIfTaskNotRunning(string taskName)
    {
        var responce = await GetTest<TaskDetailsDto>(tasksUrl + "/" + taskName);
        responce.Should().NotBeNull();
        responce!.IsRunning.Should().BeFalse();
    }

    private long CountDocumentsInIndex(string indexName)
    {
        var elasticClient = App.GetElasticClient();
        var countResponse = elasticClient.Count(new CountRequest(Indices.Index(indexName)));
        return countResponse.Count;
    }
}