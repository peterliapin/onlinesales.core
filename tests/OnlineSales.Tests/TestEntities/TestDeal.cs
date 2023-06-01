// <copyright file="TestDeal.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

namespace OnlineSales.Tests.TestEntities;

public class TestDeal : DealCreateDto
{
    public TestDeal(string uid = "", int accountId = 0, int dealPipelineId = 0, string userId = "userId")
    {
        AccountId = accountId;
        DealPipelineId = dealPipelineId;
        Currency = "USD";
        ExpectedCloseDate = new DateTime(2023, 6, 1).ToUniversalTime();
        UserId = userId;
    }
}