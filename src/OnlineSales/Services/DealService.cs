// <copyright file="DealService.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Nest;
using OnlineSales.Data;
using OnlineSales.Entities;
using OnlineSales.Interfaces;

namespace OnlineSales.Services;

public class DealService : IDealService
{
    private readonly PgDbContext pgDbContext;

    public DealService(PgDbContext pgDbContext)
    {
        this.pgDbContext = pgDbContext;
    }

    public async Task SaveAsync(Deal item)
    {
        if (item.Id > 0)
        {
            pgDbContext.Deals!.Update(item);
        }
        else
        {
            if (item.PipelineStageId <= 0)
            {
                item.PipelineStageId = GetStartStageId(item.DealPipelineId);
            }

            await pgDbContext.Deals!.AddAsync(item);
        }
    }

    public async Task SaveRangeAsync(List<Deal> items)
    {
        var pipelineIds = items.Select(i => i.DealPipelineId).ToHashSet();
        var stages = await pgDbContext.DealPipelineStages!.Where(s => pipelineIds.Contains(s.DealPipelineId)).ToListAsync();
        foreach (var item in items)
        {
            if (item.PipelineStageId <= 0)
            {
                var stage = stages.Where(s => s.DealPipelineId == item.DealPipelineId).MinBy(s => s.Order);
                if (stage != null)
                {
                    item.PipelineStageId = stage.Id;
                }
                else
                {
                    throw new DealPipelineStageException($"Pipeline with id = {item.DealPipelineId} doesn't have any stages");
                }
            }
        }

        var sortedDeals = items.GroupBy(c => c.Id > 0);

        foreach (var deal in sortedDeals)
        {
            if (deal.Key)
            {
                pgDbContext.UpdateRange(deal.ToList());
            }
            else
            {
                await pgDbContext.AddRangeAsync(deal.ToList());
            }
        }
    }

    private int GetStartStageId(int pipelineId)
    {
        var pipeline = pgDbContext.DealPipelines!.Include(p => p.PipelineStages).Where(p => p.Id == pipelineId).FirstOrDefault();
        if (pipeline == null)
        {
            throw new DealPipelineStageException($"Cannot find pipeline with id = {pipelineId}");
        }

        if (pipeline.PipelineStages == null || pipeline.PipelineStages!.Count == 0)
        {
            throw new DealPipelineStageException($"Pipeline with id = {pipelineId} doesn't have any stages");
        }

        var res = pipeline.PipelineStages!.AsEnumerable().MinBy(s => s.Order)!.Id;
        return res;
    }
}