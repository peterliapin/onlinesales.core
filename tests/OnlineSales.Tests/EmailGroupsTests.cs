// <copyright file="EmailGroupsTests.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using OnlineSales.DataAnnotations;

namespace OnlineSales.Tests;
public class EmailGroupsTests : SimpleTableTests<EmailGroup, TestEmailGroup, EmailGroupUpdateDto, ISaveService<EmailGroup>>
{
    public EmailGroupsTests()
        : base("/api/email-groups")
    {
    }

    [Fact]
    public async Task GetWithWhereLikeTest()
    {
        // we are trying to test Where query in Postgres, so we have chosen a type without elastic indexing
        Attribute.GetCustomAttribute(typeof(EmailGroup), typeof(SupportsElasticAttribute)).Should().BeNull();

        var bulkEntitiesList = new List<EmailGroup>();

        var bulkList = TestData.GenerateAndPopulateAttributes<TestEmailGroup>("1", tc => tc.Name = "1 Test");
        bulkEntitiesList.Add(this.mapper.Map<EmailGroup>(bulkList));
        bulkList = TestData.GenerateAndPopulateAttributes<TestEmailGroup>("2", tc => tc.Name = "Test 2 z");
        bulkEntitiesList.Add(this.mapper.Map<EmailGroup>(bulkList));
        bulkList = TestData.GenerateAndPopulateAttributes<TestEmailGroup>("3", tc => tc.Name = "Test 3");
        bulkEntitiesList.Add(this.mapper.Map<EmailGroup>(bulkList));
        bulkList = TestData.GenerateAndPopulateAttributes<TestEmailGroup>("4", tc => tc.Name = "Te1st 3$");
        bulkEntitiesList.Add(this.mapper.Map<EmailGroup>(bulkList));

        App.PopulateBulkData<EmailGroup, ISaveService<EmailGroup>>(bulkEntitiesList);

        var result = await this.GetTest<List<EmailGroup>>(this.itemsUrl + "?filter[where][UpdatedAt][like]=.*est", HttpStatusCode.BadRequest);
        result.Should().BeNull();

        result = await this.GetTest<List<EmailGroup>>(this.itemsUrl + "?filter[where][Name][like]=.*est");
        result!.Count.Should().Be(3);
    }

    [Fact]
    public async Task GetWithWhereEqualTest()
    {
        // we are trying to test Where query in Postgres, so we have chosen a type without elastic indexing
        Attribute.GetCustomAttribute(typeof(EmailGroup), typeof(SupportsElasticAttribute)).Should().BeNull();

        var bulkEntitiesList = new List<EmailGroup>();

        var bulkList = TestData.GenerateAndPopulateAttributes<TestEmailGroup>("1", tc => tc.Name = "Test1");
        bulkEntitiesList.Add(this.mapper.Map<EmailGroup>(bulkList));
        bulkList = TestData.GenerateAndPopulateAttributes<TestEmailGroup>("2", tc => tc.Name = "Test2");
        bulkEntitiesList.Add(this.mapper.Map<EmailGroup>(bulkList));
        bulkList = TestData.GenerateAndPopulateAttributes<TestEmailGroup>("3", tc => tc.Name = "Test3");
        bulkEntitiesList.Add(this.mapper.Map<EmailGroup>(bulkList));
        bulkList = TestData.GenerateAndPopulateAttributes<TestEmailGroup>("4", tc => tc.Name = "Tes|t4");
        bulkEntitiesList.Add(this.mapper.Map<EmailGroup>(bulkList));
        bulkList = TestData.GenerateAndPopulateAttributes<TestEmailGroup>("5", tc => tc.Name = "Test1 Test2");
        bulkEntitiesList.Add(this.mapper.Map<EmailGroup>(bulkList));

        App.PopulateBulkData<EmailGroup, ISaveService<EmailGroup>>(bulkEntitiesList);

        var result = await this.GetTest<List<EmailGroup>>(this.itemsUrl + "?filter[where][UpdatedAt][eq]=null");
        result!.Count.Should().Be(5);
        result = await this.GetTest<List<EmailGroup>>(this.itemsUrl + "?filter[where][UpdatedAt][eq]=");
        result!.Count.Should().Be(5);
        result = await this.GetTest<List<EmailGroup>>(this.itemsUrl + "?filter[where][UpdatedAt][neq]=null");
        result!.Count.Should().Be(0);
        result = await this.GetTest<List<EmailGroup>>(this.itemsUrl + "?filter[where][UpdatedAt][neq]=");
        result!.Count.Should().Be(0);
        result = await this.GetTest<List<EmailGroup>>(this.itemsUrl + "?filter[where][CreatedAt][eq]=null");
        result!.Count.Should().Be(0);
        result = await this.GetTest<List<EmailGroup>>(this.itemsUrl + "?filter[where][CreatedAt][neq]=null");
        result!.Count.Should().Be(5);
        result = await this.GetTest<List<EmailGroup>>(this.itemsUrl + "?filter[where][Name][eq]=");
        result!.Count.Should().Be(0);
        result = await this.GetTest<List<EmailGroup>>(this.itemsUrl + "?filter[where][Name][eq]=Test1|Test2");
        result!.Count.Should().Be(2);
        result = await this.GetTest<List<EmailGroup>>(this.itemsUrl + "?filter[where][Name][neq]=Test1|Test2");
        result!.Count.Should().Be(3);
        result = await this.GetTest<List<EmailGroup>>(this.itemsUrl + "?filter[where][Name][eq]=Test1");
        result!.Count.Should().Be(1);
        result = await this.GetTest<List<EmailGroup>>(this.itemsUrl + "?filter[where][Name][neq]=Test1");
        result!.Count.Should().Be(4);
        result = await this.GetTest<List<EmailGroup>>(this.itemsUrl + "?filter[where][Name][eq]=Test5|Test6");
        result!.Count.Should().Be(0);
        result = await this.GetTest<List<EmailGroup>>(this.itemsUrl + "?filter[where][Name][neq]=Test5|Test6");
        result!.Count.Should().Be(5);
        result = await this.GetTest<List<EmailGroup>>(this.itemsUrl + "?filter[where][Name][eq]=Test1|Tes\\|t4");
        result!.Count.Should().Be(2);
    }

    [Fact]
    public async Task GetWithWhereContainTest()
    {
        // we are trying to test Where query in Postgres, so we have chosen a type without elastic indexing
        Attribute.GetCustomAttribute(typeof(EmailGroup), typeof(SupportsElasticAttribute)).Should().BeNull();

        var bulkEntitiesList = new List<EmailGroup>();

        var bulkList = TestData.GenerateAndPopulateAttributes<TestEmailGroup>("1", tc => tc.Name = "1 Test");
        bulkEntitiesList.Add(this.mapper.Map<EmailGroup>(bulkList));
        bulkList = TestData.GenerateAndPopulateAttributes<TestEmailGroup>("2", tc => tc.Name = "Test 2 z");
        bulkEntitiesList.Add(this.mapper.Map<EmailGroup>(bulkList));
        bulkList = TestData.GenerateAndPopulateAttributes<TestEmailGroup>("3", tc => tc.Name = "Test 3");
        bulkEntitiesList.Add(this.mapper.Map<EmailGroup>(bulkList));
        bulkList = TestData.GenerateAndPopulateAttributes<TestEmailGroup>("4", tc => tc.Name = "Te*st 3");
        bulkEntitiesList.Add(this.mapper.Map<EmailGroup>(bulkList));

        App.PopulateBulkData<EmailGroup, ISaveService<EmailGroup>>(bulkEntitiesList);

        var result = await this.GetTest<List<EmailGroup>>(this.itemsUrl + "?filter[where][UpdatedAt][contains]=Test", HttpStatusCode.BadRequest);
        result.Should().BeNull();
        result = await this.GetTest<List<EmailGroup>>(this.itemsUrl + "?filter[where][Name][contains]=Test");
        result!.Count.Should().Be(0);
        result = await this.GetTest<List<EmailGroup>>(this.itemsUrl + "?filter[where][Name][contains]=*Test*");
        result!.Count.Should().Be(3);
        result = await this.GetTest<List<EmailGroup>>(this.itemsUrl + "?filter[where][Name][contains]=Test*");
        result!.Count.Should().Be(2);
        result = await this.GetTest<List<EmailGroup>>(this.itemsUrl + "?filter[where][Name][contains]=*Test");
        result!.Count.Should().Be(1);
        result = await this.GetTest<List<EmailGroup>>(this.itemsUrl + "?filter[where][Name][contains]=*Te\\*st*");
        result!.Count.Should().Be(1);
    }

    [Fact]
    public async Task GetWithWhereDateComparisonTest()
    {
        // we are trying to test Where query in Postgres, so we have chosen a type without elastic indexing
        Attribute.GetCustomAttribute(typeof(EmailGroup), typeof(SupportsElasticAttribute)).Should().BeNull();

        var bulkEntitiesList = new List<EmailGroup>();

        var bulkList = TestData.GenerateAndPopulateAttributes<TestEmailGroup>("1");
        bulkEntitiesList.Add(this.mapper.Map<EmailGroup>(bulkList));
        App.PopulateBulkData<EmailGroup, ISaveService<EmailGroup>>(bulkEntitiesList);

        var result = await this.GetTest<List<EmailGroup>>(this.itemsUrl + "?filter[where][CreatedAt][gt]=", HttpStatusCode.BadRequest);
        result.Should().BeNull();

        var now = DateTime.UtcNow;
        var timeStr = now.ToString("yyyy-MM-dd'T'HH:mm:ss.fffK");

        result = await this.GetTest<List<EmailGroup>>(this.itemsUrl + "?filter[where][CreatedAt][gt]=" + timeStr);
        result!.Count.Should().Be(0);
        result = await this.GetTest<List<EmailGroup>>(this.itemsUrl + "?filter[where][CreatedAt][lt]=" + timeStr);
        result!.Count.Should().Be(1);
    }

    [Fact]
    public async Task GetWithIncludeTest()
    {
        // we are trying to test Where query in Postgres, so we have chosen a type without elastic indexing
        Attribute.GetCustomAttribute(typeof(EmailGroup), typeof(SupportsElasticAttribute)).Should().BeNull();

        int numberOfTemplates = 10;

        App.PopulateBulkData<EmailGroup, ISaveService<EmailGroup>>(mapper.Map<EmailGroup>(TestData.GenerateAndPopulateAttributes<TestEmailGroup>("1", tc => tc.Name = "TestEmailGroup")));
        App.PopulateBulkData<EmailGroup, ISaveService<EmailGroup>>(mapper.Map<List<EmailTemplate>>(TestData.GenerateAndPopulateAttributes<TestEmailTemplate>(numberOfTemplates, null, 1)));

        var emailGroupResult = await GetTest<List<EmailGroupDetailsDto>>(itemsUrl + "?filter[include]=EmailTemplates");
        emailGroupResult!.Count.Should().Be(1);
        emailGroupResult[0].EmailTemplates.Should().NotBeNull();
        emailGroupResult[0].EmailTemplates!.Count.Should().Be(numberOfTemplates);
        foreach (var eg in emailGroupResult)
        {
            foreach (var et in eg.EmailTemplates!)
            {
                et.EmailGroup.Should().BeNull(); 
            }
        }

        var emailTemplateResult = await GetTest<List<EmailTemplateDetailsDto>>($"/api/email-templates?filter[include]=EmailGroup&filter[where][Id][gt]={numberOfTemplates / 2}");
        emailTemplateResult!.Count.Should().Be(numberOfTemplates / 2);
        foreach (var emailTemplate in emailTemplateResult)
        {
            emailTemplate.EmailGroup.Should().NotBeNull();
            emailTemplate.EmailGroup!.Id.Should().Be(1);
            emailTemplate.EmailGroup.EmailTemplates.Should().BeNull();
        }
    }

    protected override EmailGroupUpdateDto UpdateItem(TestEmailGroup to)
    {
        var from = new EmailGroupUpdateDto();
        to.Name = from.Name = to.Name + "Updated";
        return from;
    }
}