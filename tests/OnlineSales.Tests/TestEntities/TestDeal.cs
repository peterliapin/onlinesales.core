// <copyright file="TestDeal.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

namespace OnlineSales.Tests.TestEntities;

public class TestDeal : DealCreateDto
{
    public TestDeal(string uid, HashSet<int> contactIds, int accountId, int dealPipelineId, string userId)
    {
        AccountId = accountId;
        ContactIds = contactIds;
        DealPipelineId = dealPipelineId;
        DealCurrency = "USD";
        ExpectedCloseDate = new DateTime(2023, 6, 1).ToUniversalTime();
        UserId = userId;
    }
}