// <copyright file="PipelineStagesController.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineSales.Data;
using OnlineSales.DTOs;
using OnlineSales.Entities;
using OnlineSales.Infrastructure;

namespace OnlineSales.Controllers;

[Authorize]
[Route("api/[controller]")]
public class PipelineStagesController : BaseController<PipelineStage, PipelineStageCreateDto, PipelineStageUpdateDto, PipelineStageDetailsDto>
{
    public PipelineStagesController(PgDbContext dbContext, IMapper mapper, EsDbContext esDbContext, QueryProviderFactory<PipelineStage> queryProviderFactory)
    : base(dbContext, mapper, esDbContext, queryProviderFactory)
    {
    }
}