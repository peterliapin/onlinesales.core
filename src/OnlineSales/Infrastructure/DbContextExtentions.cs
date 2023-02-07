// <copyright file="DbContextExtentions.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace OnlineSales.Infrastructure;

public static class DbContextExtentions
{
    public static IEnumerable<IEntityType> GetRelatedEntities(this DbContext context, Type entityType)
    {
        var model = context.Model;

        var entity = model.FindEntityType(entityType);

        return entity!.GetReferencingForeignKeys().Select(fk => fk.PrincipalEntityType);
    }

    public static IEnumerable<IForeignKey> GetForeignKeys(this DbContext context, Type entityType)
    {
        var model = context.Model;

        var entity = model.FindEntityType(entityType);

        return entity!.GetForeignKeys();
    }

    public static IQueryable<object> SetDbEntity(this DbContext context, Type t)
    {
        return (IQueryable<object>)context!.GetType() !.GetMethod("Set", Type.EmptyTypes) !.MakeGenericMethod(t).Invoke(context!, null) !;
    }
}
