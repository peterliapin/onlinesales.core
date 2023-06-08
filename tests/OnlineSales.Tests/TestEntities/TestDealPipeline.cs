// <copyright file="TestDealPipeline.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

namespace OnlineSales.Tests.TestEntities;

public class TestDealPipeline : DealPipelineCreateDto
{
    public TestDealPipeline(string uid = "")
    {
        Name = $"TestDealPipeline{uid}";
    }
}