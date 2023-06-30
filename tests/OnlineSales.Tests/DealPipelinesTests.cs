// <copyright file="DealPipelinesTests.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using OnlineSales.DataAnnotations;

namespace OnlineSales.Tests;
public class DealPipelinesTests : SimpleTableTests<DealPipeline, TestDealPipeline, DealPipelineUpdateDto, ISaveService<DealPipeline>>
{
    public DealPipelinesTests()
        : base("/api/deal-pipelines")
    {
    }

    protected override DealPipelineUpdateDto UpdateItem(TestDealPipeline tdp)
    {
        var from = new DealPipelineUpdateDto();
        tdp.Name = from.Name = tdp.Name + "Updated";
        return from;
    }
}