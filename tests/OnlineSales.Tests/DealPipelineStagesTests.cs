// <copyright file="DealPipelineStagesTests.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using OnlineSales.DataAnnotations;

namespace OnlineSales.Tests;

public class DealPipelineStagesTests : TableWithFKTests<DealPipelineStage, TestPipelineStage, DealPipelineStageUpdateDto, IEntityService<DealPipelineStage>>
{
    public DealPipelineStagesTests()
        : base("/api/deal-pipeline-stages")
    {
    }

    [Fact]

    public async Task NonUniqueOrderPostTest()
    {
        var fkItem = await CreateFKItem();

        var stage = new TestPipelineStage("1", fkItem.Item1);
        stage.Order = 1;
        await PostTest(itemsUrl, stage);

        stage = new TestPipelineStage("2", fkItem.Item1);
        stage.Order = 1;
        await PostTest(itemsUrl, stage, HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task NonUniqueOrderPatchTest()
    {
        var fkItem = await CreateFKItem();

        var stage1 = new TestPipelineStage("1", fkItem.Item1);
        stage1.Order = 1;
        await PostTest(itemsUrl, stage1);

        var stage2 = new TestPipelineStage("2", fkItem.Item1);
        stage2.Order = 2;
        var location = await PostTest(itemsUrl, stage2);

        var stage = UpdateItem(stage2);
        stage.Order = 1;
        await PatchTest(location, stage, HttpStatusCode.InternalServerError);
    }

    protected override async Task<(TestPipelineStage, string)> CreateItem(string uid, int fkId)
    {
        var stage = new TestPipelineStage(uid, fkId);

        var emailTemplateUrl = await PostTest(itemsUrl, stage);

        return (stage, emailTemplateUrl);
    }

    protected override async Task<(int, string)> CreateFKItem()
    {
        var fkItemCreate = new TestDealPipeline();

        var fkUrl = await PostTest("/api/deal-pipelines", fkItemCreate, HttpStatusCode.Created);

        var fkItem = await GetTest<DealPipeline>(fkUrl);

        fkItem.Should().NotBeNull();

        return (fkItem!.Id, fkUrl);
    }

    protected override DealPipelineStageUpdateDto UpdateItem(TestPipelineStage tps)
    {
        var from = new DealPipelineStageUpdateDto();
        tps.Name = from.Name = tps.Name + "Updated";
        return from;
    }
}