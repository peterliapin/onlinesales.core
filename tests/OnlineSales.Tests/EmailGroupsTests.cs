// <copyright file="EmailGroupsTests.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using OnlineSales.DataAnnotations;

namespace OnlineSales.Tests;
public class EmailGroupsTests : SimpleTableTests<EmailGroup, TestEmailGroup, EmailGroupUpdateDto>
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
        bulkEntitiesList.Add(mapper.Map<EmailGroup>(bulkList));
        bulkList = TestData.GenerateAndPopulateAttributes<TestEmailGroup>("2", tc => tc.Name = "Test 2 z");
        bulkEntitiesList.Add(mapper.Map<EmailGroup>(bulkList));
        bulkList = TestData.GenerateAndPopulateAttributes<TestEmailGroup>("3", tc => tc.Name = "Test 3");
        bulkEntitiesList.Add(mapper.Map<EmailGroup>(bulkList));
        bulkList = TestData.GenerateAndPopulateAttributes<TestEmailGroup>("4", tc => tc.Name = "Te1st 3");
        bulkEntitiesList.Add(mapper.Map<EmailGroup>(bulkList));

        App.PopulateBulkData(bulkEntitiesList);

        var result = await GetTest<List<EmailGroup>>(itemsUrl + "?filter[where][Name][like]=.*est");
        result!.Count.Should().Be(3);
    }

    [Fact]
    public async Task GetWithIncludeTest()
    {
        // we are trying to test Where query in Postgres, so we have chosen a type without elastic indexing
        Attribute.GetCustomAttribute(typeof(EmailGroup), typeof(SupportsElasticAttribute)).Should().BeNull();

        int numberOfTemplates = 10;

        App.PopulateBulkData(mapper.Map<EmailGroup>(TestData.GenerateAndPopulateAttributes<TestEmailGroup>("1", tc => tc.Name = "TestEmailGroup")));
        App.PopulateBulkData(mapper.Map<List<EmailTemplate>>(TestData.GenerateAndPopulateAttributes<TestEmailTemplate>(numberOfTemplates, null, 1)));

        var emailGroupResult = await GetTest<List<EmailGroup>>(itemsUrl + "?filter[include]=EmailTemplates");
        emailGroupResult!.Count.Should().Be(1);
        emailGroupResult[0].EmailTemplates.Should().NotBeNull();
        emailGroupResult[0].EmailTemplates!.Count.Should().Be(numberOfTemplates);

        var emailTemplateResult = await GetTest<List<EmailTemplate>>($"/api/email-templates?filter[include]=EmailGroup&filter[where][Id][gt]={numberOfTemplates / 2}");
        emailTemplateResult!.Count.Should().Be(numberOfTemplates / 2);
        foreach (var emailTemplate in emailTemplateResult)
        {
            emailTemplate.EmailGroup.Should().NotBeNull();
            emailTemplate.EmailGroup!.Id.Should().Be(1);
        }
    }

    protected override EmailGroupUpdateDto UpdateItem(TestEmailGroup to)
    {
        var from = new EmailGroupUpdateDto();
        to.Name = from.Name = to.Name + "Updated";
        return from;
    }
}