// <copyright file="EntityNotFoundException.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Runtime.Serialization;

namespace OnlineSales.Exceptions;

[Serializable]
public class EntityNotFoundException : Exception
{
    public EntityNotFoundException(string entityType, string entityUid)
    {
        this.EntityType = entityType;
        this.EntityUid = entityUid;
    }

    public EntityNotFoundException(string? message)
        : base(message)
    {
    }

    public EntityNotFoundException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }

    protected EntityNotFoundException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }

    public string EntityType { get; init; } = string.Empty;

    public string EntityUid { get; init; } = string.Empty;
}

