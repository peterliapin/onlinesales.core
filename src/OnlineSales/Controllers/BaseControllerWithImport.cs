// <copyright file="BaseControllerWithImport.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Linq.Expressions;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Options;
using OnlineSales.Configuration;
using OnlineSales.Data;
using OnlineSales.DataAnnotations;
using OnlineSales.Entities;
using OnlineSales.Infrastructure;
using Quartz.Xml.JobSchedulingData20;

namespace OnlineSales.Controllers;

public class BaseControllerWithImport<T, TC, TU, TD, TI> : BaseController<T, TC, TU, TD>
    where T : BaseEntityWithId, new()
    where TC : class
    where TU : class
    where TD : class
    where TI : class
{
    public BaseControllerWithImport(ApiDbContext dbContext, IMapper mapper, IOptions<ApiSettingsConfig> apiSettingsConfig)
        : base(dbContext, mapper, apiSettingsConfig)
    {
    }

    public int ImportBatchSize { get; set; } = 50;

    [HttpPost]
    [Route("import")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public virtual async Task<ActionResult> Import([FromBody] List<TI> records)
    {
        // var importingRecords = GetMappedRecords(records);

        using (var transaction = dbContext.Database.BeginTransaction())
        {
            try
            {
                var importingRecords = GetMappedRecords(records);

                dbContext.IsImportRequest = true;

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
        var surrogateForeignKeyProperty = typeof(TI).GetProperties().FirstOrDefault(p => p.GetCustomAttributes(typeof(SurrogateForeignKeyAttribute), true).Length > 0);

        if (surrogateForeignKeyProperty is not null)
        {
            var customAttribute = (SurrogateForeignKeyAttribute)surrogateForeignKeyProperty!.GetCustomAttributes(typeof(SurrogateForeignKeyAttribute), true).First();

            var foreignKeyEntityType = customAttribute.RelatedType;
            var sourceForeignKey = customAttribute.SourceForeignKey;
            // var uniqueIndexOfForeignKeyEntity = customAttribute.RelatedTypeUniqeIndex;

            var recordsWithoutFk = records.Where(r => IsEmpty(r.GetType().GetProperty(sourceForeignKey) !.GetValue(r))).ToList();
            if (recordsWithoutFk.Count > 0)
            {
                // var filterValues = recordsWithoutFk.Select(r => r.GetType().GetProperty(surrogateForeignKeyProperty.Name) !.GetValue(r)).ToList();

                var parentDbSet = dbContext.SetDbEntity(foreignKeyEntityType);

                if (parentDbSet is not null)
                {
                    // todo
                }
            }
        }

        return mapper.Map<List<T>>(records);
    }

    private async Task SaveBatchChangesAsync(List<T> importingRecords)
    {
        int position = 0;

        while (position < importingRecords.Count)
        {
            var batch = importingRecords.Skip(position).Take(ImportBatchSize).ToList();

            // Find existing items by Id.
            var existingItems = dbSet.Where(t => batch.Select(b => b.Id).Contains(t.Id)).AsNoTracking().ToList();

            // Find existing items by indexes and updated batch accordingly.
            UpdateExistingItemsAndBatchItemsByIndexes(batch, existingItems);

            UpdateParentReferences(batch);

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

    private void UpdateParentReferences(List<T> batch)
    {
        var foreignKeys = dbContext.GetForeignKeys(typeof(T));

        foreach (var fk in foreignKeys)
        {
            // Get records where foreign key is not set.
            var batchWithoutFks = batch.Where(b => IsEmpty(b!.GetType() !.GetProperty(fk.Properties.First().Name) !.GetValue(b))).ToList();

            if (batchWithoutFks is not null)
            {
                // Get the parent entity type.
                var parentEntity = fk.PrincipalEntityType;

                // Get parent's unique indexes.
                var parentProperties = parentEntity!.GetProperties().Where(p => p.IsUniqueIndex());

                // If child (batchWithoutFks) has the parent unique index.
                foreach (var property in parentProperties.Select(p => p.Name).Where(property => batchWithoutFks.All(b => b.GetType().GetProperty(property) != null)))
                {
                    var filterValues = batchWithoutFks.Select(b => b.GetType().GetProperty(property) !.GetValue(b)).ToList();

                    var exp = BuildExpressionForPropertyFilter<T>(filterValues, property, parentEntity.GetType());

                    var parentRecords = dbSet.Where(exp).AsNoTracking().ToList();

                    foreach (var parentRecord in parentRecords)
                    {
                        // TODO
                    }
                }
            }
        }
    }

    private bool IsEmpty(object? value)
    {
        if (value is null)
        {
            return true;
        }
        else if (value is string str)
        {
            return string.IsNullOrEmpty(str);
        }
        else
        {
            return Convert.ToDouble(value) == 0;
        }
    }

    private void UpdateExistingItemsAndBatchItemsByIndexes(List<T> batch, List<T> existingItems)
    {
        var entityType = dbContext.Model.FindEntityType(typeof(T));

        var properties = entityType!.GetProperties();

        foreach (var prop in properties)
        {
            // Get the unique index but not the Id since its already considered.
            if (prop.IsUniqueIndex() && !prop.IsPrimaryKey())
            {
                var filterProperty = prop.Name;

                var filterValues = batch.Where(b => b.Id == 0).Select(b => b.GetType().GetProperty(filterProperty) !.GetValue(b)).ToList();

                var exp = BuildExpressionForPropertyFilter<T>(filterValues, filterProperty, typeof(T));

                // Filter dbSet<T> from unique index property and its values to find existing records.
                var filteredData = dbSet.Where(exp).AsNoTracking().ToList();

                existingItems.AddRange(filteredData);

                UpdateBatchItemId(batch, filteredData, filterProperty);
            }
        }
    }

    private Expression<Func<TA, bool>> BuildExpressionForPropertyFilter<TA>(List<object?> filterValues, string filterProperty, Type targetListType)
    {
        var parameter = Expression.Parameter(targetListType, "t");

        var property = Expression.Property(parameter, filterProperty);

        var containsMethod = typeof(List<object>).GetMethod("Contains", new[] { typeof(object) });

        var filterExpression = Expression.Call(Expression.Constant(filterValues), containsMethod!, property);

        return Expression.Lambda<Func<TA, bool>>(filterExpression, parameter);
    }

    private void UpdateBatchItemId(List<T> batch, List<T> existingItems, string filterProperty)
    {
        foreach (var item in existingItems)
        {
            // Update the Id of the batch item if record found by unique index property.
            var batchItem = GetMatchingItem(batch, item, filterProperty);
            if (batchItem is not null)
            {
                batchItem.Id = item.Id;
            }
        }
    }

    private T? GetMatchingItem(List<T> batch, T item, string filterProperty)
    {
        var exp = BuildExpressionToFindMatchingItem(item, filterProperty);

        return batch.FirstOrDefault(exp.Compile());
    }

    private Expression<Func<T, bool>> BuildExpressionToFindMatchingItem(T itemToMatch, string propertyOfItemToMatch)
    {
        var parameter = Expression.Parameter(typeof(T), "t");

        var property = Expression.Property(parameter, propertyOfItemToMatch);

        var filterValue = Expression.Constant(itemToMatch.GetType().GetProperty(propertyOfItemToMatch) !.GetValue(itemToMatch));

        var filterExpression = Expression.Equal(property, filterValue);

        return Expression.Lambda<Func<T, bool>>(filterExpression, parameter);
    }
}

