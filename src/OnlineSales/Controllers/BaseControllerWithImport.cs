// <copyright file="BaseControllerWithImport.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Linq;
using System.Linq.Expressions;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Options;
using OnlineSales.Configuration;
using OnlineSales.Data;
using OnlineSales.DataAnnotations;
using OnlineSales.DTOs;
using OnlineSales.Entities;
using OnlineSales.Infrastructure;

namespace OnlineSales.Controllers;

public class BaseControllerWithImport<T, TC, TU, TD, TI> : BaseController<T, TC, TU, TD>
    where T : BaseEntityWithId, new()
    where TC : class
    where TU : class
    where TD : class
    where TI : BaseImportDtoWithIdAndSource
{
    public BaseControllerWithImport(PgDbContext dbContext, IMapper mapper, IOptions<ApiSettingsConfig> apiSettingsConfig, EsDbContext esDbContext)
        : base(dbContext, mapper, apiSettingsConfig, esDbContext)
    {
    }

    [HttpPost]
    [Route("import")]
    [RequestSizeLimit(100 * 1024 * 1024)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public virtual async Task<ActionResult<ImportResult>> Import([FromBody] List<TI> importRecords)
    {
        var result = new ImportResult();

        var newRecords = new List<T>();
        var duplicates = new Dictionary<TI, object>();

        dbContext.IsImportRequest = true;

        var typeIdentifiersMap = BuildTypeIdentifiersMap(importRecords);

        var relatedObjectsMap = BuildRelatedObjectsMap(typeIdentifiersMap, importRecords, newRecords, duplicates);

        var relatedTObjectsMap = relatedObjectsMap[typeof(T)];

        for (var i = 0; i < importRecords.Count; i++)
        {
            var importRecord = importRecords[i];

            if (duplicates.TryGetValue(importRecord, out object? identifierValue))
            {
                result.AddError(i, $"Row number {i} has a duplicate indentification value {identifierValue} and will be skipped. Please ensure that each record has a unique key to avoid data loss.");
                continue;
            }

            BaseEntityWithId? dbRecord = null;

            foreach (var identifierProperty in relatedTObjectsMap.IdentifierPropertyNames)
            {
                var identifierPropertyInfo = importRecord.GetType().GetProperty(identifierProperty) !;

                var propertyValue = identifierPropertyInfo.GetValue(importRecord);

                if (propertyValue != null && relatedTObjectsMap[identifierProperty].TryGetValue(propertyValue, out dbRecord))
                {
                    mapper.Map(importRecord, dbRecord);
                    FixDateKindIfNeeded((T)dbRecord!);
                    break;
                }
            }

            if (dbRecord == null)
            {
                dbRecord = mapper.Map<T>(importRecord);
                FixDateKindIfNeeded((T)dbRecord!);
                newRecords.Add((T)dbRecord!);
            }

            for (var j = 0; j < relatedTObjectsMap.SurrogateKeyPropertyNames.Count; j++)
            {
                var surrogateKeyAttribute = relatedTObjectsMap.SurrogateKeyPropertyAttributes[j];
                var surrogateKeyPropertyInfo = importRecord.GetType().GetProperty(relatedTObjectsMap.SurrogateKeyPropertyNames[j]) !;

                var surrogateKeyValue = surrogateKeyPropertyInfo.GetValue(importRecord);

                BaseEntityWithId? relatedObject;

                if (surrogateKeyValue != null && surrogateKeyValue.ToString() != string.Empty)
                {
                    var relatedObjectMap = relatedObjectsMap[surrogateKeyAttribute.RelatedType];

                    if (relatedObjectMap[surrogateKeyAttribute.RelatedTypeUniqeIndex].TryGetValue(surrogateKeyValue, out relatedObject) && relatedObject != null)
                    {
                        var navigationPropertyName = surrogateKeyAttribute.SourceForeignKey.Replace("Id", string.Empty);

                        var targetNavigationPropertyInfo = dbRecord.GetType().GetProperty(navigationPropertyName);

                        if (targetNavigationPropertyInfo == null)
                        {
                            throw new ServerException($"Entity {dbRecord.GetType().Name} does not have a required navigation property '{navigationPropertyName}'");
                        }

                        targetNavigationPropertyInfo.SetValue(dbRecord, relatedObject);
                    }
                    else
                    {
                        result.AddError(i, $"Row number {i} references {surrogateKeyAttribute.RelatedType} that does not exist in the database ({surrogateKeyAttribute.RelatedTypeUniqeIndex} = {surrogateKeyValue}).");

                        if (newRecords.Contains((T)dbRecord))
                        {
                            newRecords.Remove((T)dbRecord);                            
                        }
                    }
                }
            }
        }

        await SaveRangeAsync(newRecords);

        var entriesByState = dbContext.ChangeTracker
            .Entries()
            .Where(e => e.Entity is T && (
                e.State == EntityState.Added
                || e.State == EntityState.Modified))
            .GroupBy(e => e.State)
            .ToDictionary(g => g.Key, g => g.ToList());

        result.Skipped = importRecords.Count - result.Failed;

        if (entriesByState.TryGetValue(EntityState.Added, out List<EntityEntry>? added))
        {
            result.Added = added.Count;
            result.Skipped -= result.Added;
        }

        if (entriesByState.TryGetValue(EntityState.Modified, out List<EntityEntry>? modified))
        {
            result.Updated = modified.Count;
            result.Skipped -= result.Updated;
        }

        await dbContext.SaveChangesAsync();

        return Ok(result);
    }

    protected virtual async Task SaveRangeAsync(List<T> newRecords)
    {
        await dbSet.AddRangeAsync(newRecords);
    }

    private void FixDateKindIfNeeded(T record)
    {
        var createdAtRecord = record as IHasCreatedAt;

        if (createdAtRecord != null && createdAtRecord.CreatedAt.Kind != DateTimeKind.Utc)
        {
            createdAtRecord.CreatedAt = createdAtRecord.CreatedAt.ToUniversalTime();
        }

        var updatedAtRecord = record as IHasUpdatedAt;

        if (updatedAtRecord != null && updatedAtRecord.UpdatedAt is not null && updatedAtRecord.UpdatedAt.Value.Kind != DateTimeKind.Utc)
        {
            updatedAtRecord.UpdatedAt = updatedAtRecord.UpdatedAt.Value.ToUniversalTime();
        }
    }

    private TypedRelatedObjectsMap BuildRelatedObjectsMap(TypeIdentifiers typeIdentifiersMap, List<TI> importRecords, List<T> newRecords, Dictionary<TI, object> duplicates)
    {
        var typedRelatedObjectsMap = new TypedRelatedObjectsMap();

        foreach (var type in typeIdentifiersMap.Keys)
        {
            var identifierValues = typeIdentifiersMap[type];

            var relatedObjectsMap = new RelatedObjectsMap
            {
                IdentifierPropertyNames = identifierValues.IdentifierPropertyNames,
                SurrogateKeyPropertyNames = identifierValues.SurrogateKeyPropertyNames,
                SurrogateKeyPropertyAttributes = identifierValues.SurrogateKeyPropertyAttributes,
            };

            var mappedObjectsCash = new Dictionary<TI, object>();

            foreach (var propertyName in identifierValues.Keys)
            {
                var existingRecordsProperty = type.GetProperty(propertyName) !;
                var importRecordsProperty = typeof(TI).GetProperty(propertyName) !;

                var propertyValues = identifierValues[propertyName];

                var predicate = BuildPropertyValuesPredicate(type, propertyName, propertyValues);

                var existingObjectsDict = dbContext.SetDbEntity(type)
                                        .Where(predicate).AsQueryable()
                                        .ToDictionary(x => existingRecordsProperty.GetValue(x) !, x => x);

                Dictionary<object, TI>? importRecordsDict = null;

                if (type == typeof(T))
                {
                    var uniqueGroups = importRecords
                                        .Select(x => new { Identifier = importRecordsProperty.GetValue(x), Record = x })
                                        .Where(x => x.Identifier != null && x.Identifier.ToString() != "0" && x.Identifier.ToString() != string.Empty)
                                        .GroupBy(x => x.Identifier!);

                    importRecordsDict = uniqueGroups.ToDictionary(g => g.Key, g => g.First().Record);

                    duplicates.AddRangeIfNotExists(uniqueGroups
                                        .Where(g => g.Count() > 1)
                                        .SelectMany(g => g.Skip(1))
                                        .ToDictionary(x => x.Record, x => x.Identifier!));
                }                

                relatedObjectsMap[propertyName] = propertyValues
                       .Select(uid =>
                        {
                            existingObjectsDict.TryGetValue(uid, out object? record);

                            if (type == typeof(T) && importRecordsDict!.TryGetValue(uid, out TI? importRecord))
                            {
                                if (record == null && !mappedObjectsCash.TryGetValue(importRecord, out record))
                                {
                                    record = mapper.Map<T>(importRecord);
                                    FixDateKindIfNeeded((T)record);
                                    newRecords.Add((T)record);
                                }

                                mappedObjectsCash[importRecord] = record;
                            }

                            return new { Uid = uid, Record = record };
                        })
                       .ToDictionary(x => x.Uid, x => x.Record as BaseEntityWithId);
            }

            typedRelatedObjectsMap[type] = relatedObjectsMap;
        }

        return typedRelatedObjectsMap;
    }

    private TypeIdentifiers BuildTypeIdentifiersMap(List<TI> importRecords)
    {
        var typeIdentifiersMap = new TypeIdentifiers
        {
            { typeof(T), new IdentifierValues() },
        };

        var idValues = importRecords
                    .Where(r => r.Id is not null && r.Id > 0)
                    .Select(r => (object)r.Id!.Value)
                    .Distinct()
                    .ToList();

        if (idValues.Count > 0)
        {
            typeIdentifiersMap[typeof(T)]["Id"] = idValues;
            typeIdentifiersMap[typeof(T)].IdentifierPropertyNames.Add("Id");
        }

        var uniqueIndexPropertyName = FindAlternateKeyPropertyName();

        if (uniqueIndexPropertyName != null)
        {
            var property = typeof(TI).GetProperty(uniqueIndexPropertyName) !;

            var uniqueValues = importRecords
                                   .Where(r => property.GetValue(r) != null && property.GetValue(r) !.ToString() != string.Empty)
                                   .Select(r => property.GetValue(r))
                                   .Distinct()
                                   .ToList();

            if (uniqueValues.Count > 0)
            {
                typeIdentifiersMap[typeof(T)][uniqueIndexPropertyName] = uniqueValues!;
                typeIdentifiersMap[typeof(T)].IdentifierPropertyNames.Add(uniqueIndexPropertyName);
            }
        }

        var importProperties = typeof(TI).GetProperties();

        foreach (var property in importProperties)
        {
            var surrpogateForeignKeyAttribute = property.GetCustomAttributes(typeof(SurrogateForeignKeyAttribute), true).FirstOrDefault() as SurrogateForeignKeyAttribute;

            if (surrpogateForeignKeyAttribute == null)
            {
                continue;
            }

            var type = surrpogateForeignKeyAttribute.RelatedType;

            var identifierName = surrpogateForeignKeyAttribute.RelatedTypeUniqeIndex;

            var identifierValues = importRecords
                                   .Where(r => property.GetValue(r) != null && property.GetValue(r) !.ToString() != string.Empty)
                                   .Select(r => property.GetValue(r))
                                   .Distinct()
                                   .ToList();

            if (identifierValues.Count == 0)
            {
                continue;
            }

            if (!typeIdentifiersMap.ContainsKey(type))
            {
                typeIdentifiersMap[type] = new IdentifierValues();
            }

            if (!typeIdentifiersMap[type].ContainsKey(identifierName))
            {
                typeIdentifiersMap[type][identifierName] = new List<object>();
            }

            typeIdentifiersMap[type][identifierName].AddRange(identifierValues!);

            typeIdentifiersMap[type][identifierName] = typeIdentifiersMap[type][identifierName].Distinct().ToList();

            typeIdentifiersMap[typeof(T)].SurrogateKeyPropertyNames.Add(property.Name);
            typeIdentifiersMap[typeof(T)].SurrogateKeyPropertyAttributes.Add(surrpogateForeignKeyAttribute);
        }

        return typeIdentifiersMap;
    }

    private string FindAlternateKeyPropertyName()
    {
        var uniqueIndexPropertyName = typeof(T).GetCustomAttributes(typeof(IndexAttribute), true)
                               .Select(a => (IndexAttribute)a)
                               .Where(a => a.IsUnique)
                               .Select(a => a.PropertyNames[0]) // for now the assumption is that we do not support composite indexes
                               .FirstOrDefault(); // and we only support a single index per entity

        if (uniqueIndexPropertyName is null)
        {
            uniqueIndexPropertyName = typeof(T).GetCustomAttributes(typeof(SurrogateIdentityAttribute), true)
                                   .Select(a => (SurrogateIdentityAttribute)a)
                                   .Select(a => a.PropertyName) // for now the assumption is that we do not support composite indexes
                                   .FirstOrDefault(); // and we only support a single index per entity
        }

        return uniqueIndexPropertyName!;
    }

    private Func<object, bool> BuildPropertyValuesPredicate(Type targetType, string propertyName, List<object> propertyValues)
    {
        // Get the property info for the property name
        var propertyInfo = targetType.GetProperty(propertyName);

        // Create a parameter expression for the object type
        var objectParam = Expression.Parameter(typeof(object), "o");

        // Convert the object parameter to the target type
        var convertedParam = Expression.Convert(objectParam, targetType);

        // Create the property access expression for the property name
        var propertyAccess = Expression.Property(convertedParam, propertyInfo!);

        // Convert the property access expression to type object
        var convertedPropertyAccess = Expression.Convert(propertyAccess, typeof(object));

        // Create the constant expression for the property values
        var valuesConstant = Expression.Constant(propertyValues, typeof(List<object>));
        var containsMethod = typeof(List<object>).GetMethod("Contains", new[] { typeof(object) });
        var containsExpression = Expression.Call(valuesConstant, containsMethod!, convertedPropertyAccess);

        // Create the lambda expression for the predicate
        var lambdaExpression = Expression.Lambda<Func<object, bool>>(containsExpression, objectParam);

        return lambdaExpression.Compile();
    }
}

public class ImportResult
{
    public int Added { get; set; }

    public int Updated { get; set; }

    public int Failed { get; set; }

    public int Skipped { get; set; }

    public List<ImportError>? Errors { get; set; }

    public void AddError(int row, string message)
    {
        Failed++;
        Errors = Errors ?? new List<ImportError>();

        Errors.Add(new ImportError
        {
            Row = row,
            Message = message,
        });
    }
}

public class ImportError
{
    public int Row { get; set; }

    public string Message { get; set; } = string.Empty;
}

internal class TypedRelatedObjectsMap : Dictionary<Type, RelatedObjectsMap>
{
}

internal class RelatedObjectsMap : Dictionary<string, Dictionary<object, BaseEntityWithId?>>
{
    public List<string> IdentifierPropertyNames { get; set; } = new List<string>();

    public List<string> SurrogateKeyPropertyNames { get; set; } = new List<string>();

    public List<SurrogateForeignKeyAttribute> SurrogateKeyPropertyAttributes { get; set; } = new List<SurrogateForeignKeyAttribute>();
}

internal class TypeIdentifiers : Dictionary<Type, IdentifierValues>
{
}

internal class IdentifierValues : Dictionary<string, List<object>>
{
    public List<string> IdentifierPropertyNames { get; set; } = new List<string>();

    public List<string> SurrogateKeyPropertyNames { get; set; } = new List<string>();

    public List<SurrogateForeignKeyAttribute> SurrogateKeyPropertyAttributes { get; set; } = new List<SurrogateForeignKeyAttribute>();
}