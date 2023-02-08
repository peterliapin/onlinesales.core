// <copyright file="BaseControllerWithImport.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Linq.Expressions;
using System.Reflection;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OnlineSales.Configuration;
using OnlineSales.Data;
using OnlineSales.DataAnnotations;
using OnlineSales.Entities;
using OnlineSales.Infrastructure;

namespace OnlineSales.Controllers;

public class BaseControllerWithImport<T, TC, TU, TD, TI> : BaseController<T, TC, TU, TD>
    where T : BaseEntityWithId, new()
    where TC : class
    where TU : class
    where TD : class
    where TI : class
{
    public BaseControllerWithImport(PgDbContext dbContext, IMapper mapper, IOptions<ApiSettingsConfig> apiSettingsConfig)
        : base(dbContext, mapper, apiSettingsConfig)
    {
    }

    public int ImportBatchSize { get; set; } = 50;

    public List<TI>? AlternateKeyRelationList { get; set; }

    public AlternateKeyAttribute? AlternateKeyCustomAttribute { get; set; }

    public PropertyInfo? AlternateKey { get; set; }

    [HttpPost]
    [Route("import")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public virtual async Task<ActionResult> Import([FromBody] List<TI> records)
    {
        AlternateKeyRelationList = GetRecordsWithAlternateKey(records);

        var importingRecords = GetMappedRecords(records);

        using (var transaction = dbContext.Database.BeginTransaction())
        {
            try
            {
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
        UpdateParentBySurrogateForeignKey(records);

        return mapper.Map<List<T>>(records);
    }

    /// <summary>
    /// When a foreign key value is not provided, an alternative foreign key is used to
    /// to query the parent and retrieve the value of foreign key.
    /// </summary>
    protected virtual void UpdateParentBySurrogateForeignKey(List<TI> records)
    {
        try
        {
            // Check whether surrogate foreign key is available or not
            var surrogateForeignKeyProperty = typeof(TI).GetProperties().FirstOrDefault(p => p.GetCustomAttributes(typeof(SurrogateForeignKeyAttribute), true).Length > 0);

            if (surrogateForeignKeyProperty is not null)
            {
                var customAttribute = (SurrogateForeignKeyAttribute)surrogateForeignKeyProperty!.GetCustomAttributes(typeof(SurrogateForeignKeyAttribute), true).First();

                var foreignKeyEntityType = customAttribute.RelatedType;
                var foreignKeyEntityUniqueIndex = customAttribute.RelatedTypeUniqeIndex;
                var sourceForeignKey = customAttribute.SourceForeignKey;

                // Get the records which do not have a value for foreign key
                var recordsWithoutFk = records.Where(r => IsEmpty(GetValueByPropertyName(r, sourceForeignKey))).ToList();
                if (recordsWithoutFk.Count > 0)
                {
                    // Get list of surrogate foreign key values. (ex: list of postSlug in comments)
                    var uniqueKeyValues = recordsWithoutFk.Select(r => GetValueByPropertyName(r, surrogateForeignKeyProperty.Name)).ToList();

                    if (uniqueKeyValues.Count > 0)
                    {
                        // Need to query the foreign key entity
                        var parentDbSet = dbContext.SetDbEntity(foreignKeyEntityType).AsNoTracking().ToList();

                        if (parentDbSet is not null)
                        {
                            // Get the foreing key entities matching surrogate foreign key values (ex: Posts where slug equals to postslug in comments)
                            var parentListByUniqueKey = parentDbSet.Where(r => uniqueKeyValues!.Contains(GetValueByPropertyName(r, foreignKeyEntityUniqueIndex)));

                            if (parentListByUniqueKey is not null)
                            {
                                foreach (var record in recordsWithoutFk)
                                {
                                    // Find the parent using surrogate fk and update the main fk with parent id.
                                    var matchingParent = parentListByUniqueKey!.Where(p => GetValueByPropertyName(p, foreignKeyEntityUniqueIndex) !.ToString() == GetValueByPropertyName(record, surrogateForeignKeyProperty.Name) !.ToString());
                                    record.GetType() !.GetProperty(sourceForeignKey) !.SetValue(record, Convert.ToInt32(matchingParent.Select(i => GetValueByPropertyName(i, "Id")).First()));
                                }
                            }
                        }
                    }
                    else
                    {
                        string message = "Either foreign key or a surrogate foreign key should be present in a importing record.";

                        Log.Error(message);
                        throw new InvalidImportFileException(message);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error while updating parent references.");
            throw;
        }
    }

    protected virtual async Task SaveBatchChangesAsync(List<T> importingRecords)
    {
        int position = 0;

        while (position < importingRecords.Count)
        {
            var batch = importingRecords.Skip(position).Take(ImportBatchSize).ToList();

            // Find existing items by Id.
            var existingItems = dbSet.Where(t => batch.Select(b => b.Id).Contains(t.Id)).AsNoTracking().ToList();

            // Find existing items by indexes and updated batch accordingly.
            UpdateExistingItemsAndBatchItemsByIndexes(batch, existingItems);

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

            await UpdateParentByAlternateKey(batch);

            position += ImportBatchSize;
        }
    }

    /// <summary>
    /// Find the import records which contain an alternate key to relate to its own parent
    /// and do not contain a parent id at the import.
    /// </summary>
    private List<TI>? GetRecordsWithAlternateKey(List<TI> records)
    {
        List<TI> alternateKeyRelationList = new List<TI>();
        // Check whether alternate key is available or not
        AlternateKey = typeof(TI).GetProperties().FirstOrDefault(p => p.GetCustomAttributes(typeof(AlternateKeyAttribute), true).Length > 0);

        if (AlternateKey is not null)
        {
            AlternateKeyCustomAttribute = (AlternateKeyAttribute)AlternateKey!.GetCustomAttributes(typeof(AlternateKeyAttribute), true).First();

            var sourceParentIdProperty = AlternateKeyCustomAttribute.SourceParentIdProperty;

            // Get the records which do not have a value for parent id (ex: ParentId of CommentImportDto is not provided)
            var recordsWithoutFk = records.Where(r => IsEmpty(GetValueByPropertyName(r, sourceParentIdProperty) !)).ToList();
            if (recordsWithoutFk.Count > 0)
            {
                foreach (var record in recordsWithoutFk)
                {
                    // Get the value of the alternate key (ex: value of 'ParentKey' of CommentImportDto)
                    var keyValue = GetValueByPropertyName(record, AlternateKey.Name);

                    // foreign key is not provided but parent key is provided.
                    if (!IsEmpty(keyValue))
                    {
                        // Once all records are saved, this list should be reconsidered to update the parent id (ex: to update the ParentId of comment)
                        alternateKeyRelationList.Add(record);
                    }
                }
            }
        }

        return alternateKeyRelationList;
    }

    /// <summary>
    /// Items which do not have a parent id but a parent key is provided at the import
    /// will be updated with the parent id.
    /// </summary>
    private async Task UpdateParentByAlternateKey(List<T> batch)
    {
        if (AlternateKeyRelationList is null || AlternateKeyRelationList.Count == 0)
        {
            return;
        }

        var parentUniqueIndex = AlternateKeyCustomAttribute!.ParentUniqueIndex;
        var sourceForeignKey = AlternateKeyCustomAttribute!.SourceParentIdProperty;

        var entityList = dbSet.AsNoTracking().AsEnumerable();

        // Batch items are already saved in the database hense it has the id.
        foreach (var item in batch)
        {
            var sourceForeignKeyValue = GetValueByPropertyName(item, sourceForeignKey);

            // If parent id is empty.
            if (IsEmpty(sourceForeignKeyValue))
            {
                var key = GetValueByPropertyName(item, parentUniqueIndex) !;

                // Find the corresponding importing (TI) record by Key.
                var matchingImportingRecord = AlternateKeyRelationList.FirstOrDefault(a => GetValueByPropertyName(a, parentUniqueIndex) !.ToString() == key.ToString());
                if (matchingImportingRecord is null)
                {
                    continue;
                }

                var alternateKeyOfMatchingImportRecord = GetValueByPropertyName(matchingImportingRecord, AlternateKey!.Name);
                // Check whether alternate key (Parent Key) is provided.
                if (alternateKeyOfMatchingImportRecord is not null)
                {
                    // Find the parent item (T) by alternate key.
                    var parentItem = entityList.FirstOrDefault(b => GetValueByPropertyName(b, parentUniqueIndex) !.ToString() == alternateKeyOfMatchingImportRecord.ToString());
                    if (parentItem is null)
                    {
                        var message = "No parent entity is available for given alternate key";
                        Log.Error(message);
                        throw new InvalidImportFileException(message);
                    }

                    // Update the current item (T) by parent item id.
                    item.GetType().GetProperty(sourceForeignKey) !.SetValue(item, parentItem!.Id);
                    dbSet.Update(item);
                }
            }
        }

        await dbContext.SaveChangesAsync();
    }

    private object? GetValueByPropertyName(object r, string name)
    {
        var property = r.GetType().GetProperty(name);
        if (property is not null)
        {
            return property.GetValue(r) !; 
        }

        return null;
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

