// <copyright file="EntityNotFoundException.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

namespace OnlineSales.Exceptions;

[Serializable]
public class EntityNotFoundException : Exception
{
    public EntityNotFoundException(string entityType, string entityUid)
    {
        EntityType = entityType;
        EntityUid = entityUid;
    }

    public string EntityType { get; init; }

    public string EntityUid { get; init; }
}