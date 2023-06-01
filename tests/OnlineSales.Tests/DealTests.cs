// <copyright file="DealTests.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Security.Policy;
using Npgsql.TypeMapping;
using OnlineSales.Helpers;

namespace OnlineSales.Tests;

public class DealTests : SimpleTableTests<Deal, TestDeal, DealUpdateDto, IDealService>
{
    public DealTests()
        : base("/api/deals")
    {
    }

    protected override async Task<(TestDeal, string)> CreateItem(string authToken = "Success")
    {
        var fkData = await CreateFKItems(authToken);

        var dealCreate = new TestDeal(string.Empty, fkData.AccountId, fkData.PipelineId, fkData.UserId);
        var dealUrl = await PostTest(itemsUrl, dealCreate);

        return (dealCreate, dealUrl);
    }

    protected override void GenerateBulkRecords(int dataCount, Action<TestDeal>? populateAttributes = null)
    {
        var fkData = CreateFKItems().Result;

        var bulkList = TestData.GenerateAndPopulateAttributes<TestDeal>(dataCount, populateAttributes, fkData.AccountId, fkData.PipelineId, fkData.UserId);
        var bulkEntitiesList = mapper.Map<List<Deal>>(bulkList);

        App.PopulateBulkData<Deal, IDealService>(bulkEntitiesList);
    }

    protected override DealUpdateDto UpdateItem(TestDeal td)
    {
        var from = new DealUpdateDto();
        from.ExpectedCloseDate = td.ExpectedCloseDate.AddDays(1);
        td.ExpectedCloseDate = from.ExpectedCloseDate.Value;
        return from;
    }

    private async Task<string> PostUserTest(object payload)
    {
        var url = "/api/users";
        var response = await Request(HttpMethod.Post, url, payload, "Success");
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var location = response.Headers?.Location?.LocalPath ?? string.Empty;
        location.Should().StartWith(url);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonHelper.Deserialize<User>(content);
        result.Should().NotBeNull();
        result!.Id.Should().NotBeEmpty();

        return location;
    }

    private async Task<FKData> CreateFKItems(string authToken = "Success")
    {
        var accountCreate = new TestAccount();
        var accountUrl = await PostTest("/api/accounts", accountCreate, HttpStatusCode.Created, authToken);
        var account = await GetTest<Account>(accountUrl);
        account.Should().NotBeNull();

        var pipelineCreate = new TestDealPipeline();
        var pipelineUrl = await PostTest("/api/deal-pipelines", pipelineCreate, HttpStatusCode.Created, authToken);
        var pipeline = await GetTest<Account>(pipelineUrl);
        pipeline.Should().NotBeNull();

        var userCreate = new UserCreateDto() { Email = "email@gmail.com", UserName = "User", DisplayName = "DisplayName" };
        var userUrl = await PostUserTest(userCreate);
        var user = await GetTest<User>(userUrl);
        user.Should().NotBeNull();

        var stages = new TestPipelineStage[] { new TestPipelineStage(string.Empty, pipeline!.Id) { Order = 0 }, new TestPipelineStage(string.Empty, pipeline!.Id) { Order = 1 } };
        foreach (var stage in stages)
        {
            var stageUrl = await PostTest("/api/pipeline-stages", stage, HttpStatusCode.Created, authToken);
            var newStage = await GetTest<PipelineStage>(stageUrl);
            newStage.Should().NotBeNull();
        }

        return new FKData() { AccountId = account!.Id, PipelineId = pipeline!.Id, UserId = user!.Id };
    }

    private sealed class FKData
    {
        public int AccountId { get; set; }

        public int PipelineId { get; set; }

        public string UserId { get; set; } = string.Empty;
    }
}