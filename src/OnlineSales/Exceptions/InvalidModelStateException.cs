// <copyright file="InvalidModelStateException.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Runtime.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OnlineSales.Exceptions;

[Serializable]
public class InvalidModelStateException : Exception
{
    public InvalidModelStateException(ModelStateDictionary modelState)
    {
        ModelState = modelState;
    }

    public InvalidModelStateException(string? message)
        : base(message)
    {
    }

    public InvalidModelStateException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }

    protected InvalidModelStateException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }

    public ModelStateDictionary? ModelState { get; init; }
}

