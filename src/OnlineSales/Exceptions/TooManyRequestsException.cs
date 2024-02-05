// <copyright file="TooManyRequestsException.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Runtime.Serialization;

namespace OnlineSales.Exceptions;

[Serializable]
public class TooManyRequestsException : Exception
{
    public TooManyRequestsException()
    {
    }

    public TooManyRequestsException(string? message)
        : base(message)
    {
    }

    public TooManyRequestsException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }

    protected TooManyRequestsException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
