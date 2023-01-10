// <copyright file="BaseControllerWithImport.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OnlineSales.Configuration;
using OnlineSales.Data;
using OnlineSales.Entities;

namespace OnlineSales.Controllers;

public class BaseControllerWithImport<T, TC, TU, TD, TI> : BaseController<T, TC, TU, TD>
    where T : BaseEntity, new()
    where TC : class
    where TU : class
    where TD : class
    where TI : class
{
    public BaseControllerWithImport(ApiDbContext dbContext, IMapper mapper, IOptions<ApiSettingsConfig> apiSettingsConfig)
        : base(dbContext, mapper, apiSettingsConfig)
    {
    }

    public int ImportBatchSize { get; set; } = 5;

    public long FileSizeInMB { get; set; } = 5;

    [HttpPost]
    [Route("import")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public virtual async Task<ActionResult> Import([FromBody] List<TI> records)
    {
        var importingRecords = GetMappedRecords(records);

        using (var transaction = dbContext.Database.BeginTransaction())
        {
            try
            {
                await SaveBatchChangesAsync(importingRecords);

                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                Log.Error(ex, "Error when executing batch import");

                throw;
            }
        }

        return Ok();
    }

    protected virtual List<T> GetMappedRecords(List<TI> records)
    {
        return mapper.Map<List<T>>(records);
    }

    private async Task SaveBatchChangesAsync(List<T> importingRecords)
    {
        int position = 0;

        while (position < importingRecords.Count)
        {
            var batch = importingRecords.Skip(position).Take(ImportBatchSize).ToList();

            var existingItems = dbSet.Where(t => batch.Select(b => b.Id).Contains(t.Id)).AsNoTracking().ToList();

            foreach (var item in batch)
            {
                if (existingItems.Any(t => t.Id == item.Id))
                {
                    dbSet.Update(item);
                }
                else
                {
                    await dbSet.AddAsync(item);
                }
            }

            await dbContext.SaveChangesAsync();

            position += ImportBatchSize;
        }
    }
}

