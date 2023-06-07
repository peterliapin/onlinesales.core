// <copyright file="DealTests.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using OnlineSales.Helpers;

namespace OnlineSales.Tests;

public class DealTests : SimpleTableTests<Deal, TestDeal, DealUpdateDto, IDealService>
{
    public DealTests()
        : base("/api/deals")
    {
    }

    [Fact]
    public async Task CreateAndUpdateItemTestWithContacts()
    {
        // successful creation
        var testContacts = new List<TestContact>() { new TestContact("1"), new TestContact("2") };
        var fkData = await CreateFKItems(testContacts, "Success");
        var dealCreate = new TestDeal(string.Empty, fkData.ContactIds, fkData.AccountId, fkData.PipelineId, fkData.UserId);
        var url = await PostTest(itemsUrl, dealCreate);
        var items = await GetTest<List<DealDetailsDto>>("/api/deals?filter[include]=Contacts", HttpStatusCode.OK, "Success");
        items!.Count.Should().Be(1);
        items[0].Contacts!.Select(c => c.Email).Should().BeEquivalentTo(testContacts.Select(tc => tc.Email));
        var existedContactsIds = items[0].Contacts!.Select(c => c.Id).ToList();

        // failed creation
        dealCreate = new TestDeal(string.Empty, new HashSet<int>() { existedContactsIds.Max() + 1 }, fkData.AccountId, fkData.PipelineId, fkData.UserId);
        await PostTest(itemsUrl, dealCreate, HttpStatusCode.NotFound);

        // successful patching
        fkData.ContactIds.Remove(fkData.ContactIds.First());
        var dealUpdate = new DealUpdateDto() { ContactIds = fkData.ContactIds };
        await PatchTest(url, dealUpdate);
        items = await GetTest<List<DealDetailsDto>>("/api/deals?filter[include]=Contacts", HttpStatusCode.OK, "Success");
        items!.Count.Should().Be(1);
        items[0].Contacts!.Select(c => c.Id).Should().BeEquivalentTo(fkData.ContactIds);

        // failed patching
        dealUpdate = new DealUpdateDto() { ContactIds = new HashSet<int>() { existedContactsIds.Max() + 1 } };
        await PatchTest(url, dealUpdate, HttpStatusCode.NotFound);
    }

    protected override async Task<(TestDeal, string)> CreateItem(string authToken = "Success")
    {
        var fkData = await CreateFKItems(new List<TestContact>(), authToken);

        var dealCreate = new TestDeal(string.Empty, fkData.ContactIds, fkData.AccountId, fkData.PipelineId, fkData.UserId);
        var dealUrl = await PostTest(itemsUrl, dealCreate);

        return (dealCreate, dealUrl);
    }

    protected override void GenerateBulkRecords(int dataCount, Action<TestDeal>? populateAttributes = null)
    {
        var fkData = CreateFKItems(new List<TestContact>()).Result;

        var bulkList = TestData.GenerateAndPopulateAttributes<TestDeal>(dataCount, populateAttributes, fkData.ContactIds, fkData.AccountId, fkData.PipelineId, fkData.UserId);
        var bulkEntitiesList = mapper.Map<List<Deal>>(bulkList);

        App.PopulateBulkData<Deal, IDealService>(bulkEntitiesList);
    }

    protected override DealUpdateDto UpdateItem(TestDeal td)
    {
        var from = new DealUpdateDto();
        if (td.ExpectedCloseDate.HasValue)
        {
            td.ExpectedCloseDate = from.ExpectedCloseDate = td.ExpectedCloseDate.Value.AddDays(1);
        }
        else
        {
            td.ExpectedCloseDate = from.ExpectedCloseDate = DateTime.UtcNow;
        }

        return from;
    }

    protected override void MustBeEquivalent(object? expected, object? result)
    {
        result.Should().BeEquivalentTo(expected, options => options.Excluding(o => ((TestDeal)o!).ContactIds));
        var resultDeal = (Deal)result!;
        if (resultDeal.Contacts != null)
        {
            var expectedContactdOds = ((TestDeal)expected!).ContactIds;
            var resultContactdIds = resultDeal.Contacts!.Select(c => c.Id).ToHashSet();
            resultContactdIds.Should().BeEquivalentTo(expectedContactdOds);
        }
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

    private async Task<FKData> CreateFKItems(List<TestContact> testContacts, string authToken = "Success")
    {
        var result = new FKData();

        var accountCreate = new TestAccount();
        var accountUrl = await PostTest("/api/accounts", accountCreate, HttpStatusCode.Created, authToken);
        var account = await GetTest<Account>(accountUrl);
        account.Should().NotBeNull();
        result.AccountId = account!.Id;

        var pipelineCreate = new TestDealPipeline();
        var pipelineUrl = await PostTest("/api/deal-pipelines", pipelineCreate, HttpStatusCode.Created, authToken);
        var pipeline = await GetTest<Account>(pipelineUrl);
        pipeline.Should().NotBeNull();
        result.PipelineId = pipeline!.Id;

        var userCreate = new UserCreateDto() { Email = "email@gmail.com", UserName = "User", DisplayName = "DisplayName" };
        var userUrl = await PostUserTest(userCreate);
        var user = await GetTest<User>(userUrl);
        user.Should().NotBeNull();
        result.UserId = user!.Id;

        var stages = new TestPipelineStage[] { new TestPipelineStage(string.Empty, pipeline!.Id) { Order = 0 }, new TestPipelineStage(string.Empty, pipeline!.Id) { Order = 1 } };
        foreach (var stage in stages)
        {
            var stageUrl = await PostTest("/api/deal-pipeline-stages", stage, HttpStatusCode.Created, authToken);
            var newStage = await GetTest<DealPipelineStage>(stageUrl);
            newStage.Should().NotBeNull();
        }

        foreach (var contact in testContacts)
        {
            var contactUrl = await PostTest("/api/contacts", contact, HttpStatusCode.Created, authToken);
            var newContact = await GetTest<Contact>(contactUrl);
            result.ContactIds.Add(newContact!.Id);
        }

        return result;
    }

    private sealed class FKData
    {
        public int AccountId { get; set; }

        public int PipelineId { get; set; }

        public string UserId { get; set; } = string.Empty;

        public HashSet<int> ContactIds { get; set; } = new HashSet<int>();
    }
}