// <copyright file="EmailGroupsTests.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using FluentAssertions;
using OnlineSales.DataAnnotations;
using OnlineSales.DTOs;
using OnlineSales.Entities;

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

        var result = await GetTest<List<Order>>(itemsUrl + "?filter[where][Name][like]=.*est");
        result!.Count.Should().Be(3);
    }

    protected override EmailGroupUpdateDto UpdateItem(TestEmailGroup to)
    {
        var from = new EmailGroupUpdateDto();
        to.Name = from.Name = to.Name + "Updated";
        return from;
    }
}