// <copyright file="DealPipelineStagesController.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineSales.Data;
using OnlineSales.DTOs;
using OnlineSales.Entities;
using OnlineSales.Infrastructure;
using OnlineSales.Services;

namespace OnlineSales.Controllers;

[Authorize]
[Route("api/[controller]")]
public class DealPipelineStagesController : BaseController<DealPipelineStage, DealPipelineStageCreateDto, DealPipelineStageUpdateDto, DealPipelineStageDetailsDto>
{
    public DealPipelineStagesController(PgDbContext dbContext, IMapper mapper, EsDbContext esDbContext, QueryProviderFactory<DealPipelineStage> queryProviderFactory)
    : base(dbContext, mapper, esDbContext, queryProviderFactory)
    {
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public override async Task<ActionResult<DealPipelineStageDetailsDto>> Post([FromBody] DealPipelineStageCreateDto value)
    {
        CheckOrderUnique(value.DealPipelineId, value.Order);
        return await base.Post(value);
    }

    [HttpPatch("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public override async Task<ActionResult<DealPipelineStageDetailsDto>> Patch(int id, [FromBody] DealPipelineStageUpdateDto value)
    {
        var existingEntity = await FindOrThrowNotFound(id);

        if ((value.DealPipelineId.HasValue && value.DealPipelineId.Value != existingEntity.DealPipelineId)
            || (value.Order.HasValue && value.Order.Value != existingEntity.Order))
        {
            var pipelineId = value.DealPipelineId.HasValue ? value.DealPipelineId.Value : existingEntity.DealPipelineId;
            var stageOrder = value.Order.HasValue ? value.Order.Value : existingEntity.Order;
            CheckOrderUnique(pipelineId, stageOrder);
        }

        return await Patch(existingEntity, value);
    }

    private void CheckOrderUnique(int pipelineId, int stageOrder)
    {
        if (dbContext.DealPipelineStages!.Any(s => s.DealPipelineId == pipelineId && s.Order == stageOrder))
        {
            throw new DealPipelineStageException($"Pipeline stage with pipelineId = {pipelineId} and Order = {stageOrder} is already exist");
        }
    }
}