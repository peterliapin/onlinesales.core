// <copyright file="PipelineStagesTests.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using OnlineSales.DataAnnotations;

namespace OnlineSales.Tests;

public class PipelineStagesTests : TableWithFKTests<DealPipelineStage, TestPipelineStage, PipelineStageUpdateDto, ISaveService<DealPipelineStage>>
{
    public PipelineStagesTests()
        : base("/api/pipeline-stages")
    {
    }

    protected override async Task<(TestPipelineStage, string)> CreateItem(string uid, int fkId)
    {
        var stage = new TestPipelineStage(uid, fkId);

        var emailTemplateUrl = await PostTest(itemsUrl, stage);

        return (stage, emailTemplateUrl);
    }

    protected override async Task<(int, string)> CreateFKItem(string authToken = "Success")
    {
        var fkItemCreate = new TestDealPipeline();

        var fkUrl = await PostTest("/api/deal-pipelines", fkItemCreate, HttpStatusCode.Created, authToken);

        var fkItem = await GetTest<DealPipeline>(fkUrl);

        fkItem.Should().NotBeNull();

        return (fkItem!.Id, fkUrl);
    }

    protected override PipelineStageUpdateDto UpdateItem(TestPipelineStage tps)
    {
        var from = new PipelineStageUpdateDto();
        tps.Name = from.Name = tps.Name + "Updated";
        return from;
    }
}