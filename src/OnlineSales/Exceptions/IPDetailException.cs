// <copyright file="IPDetailException.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Runtime.Serialization;

namespace OnlineSales.Exceptions;

[Serializable]
public class IPDetailException : Exception
{
    public IPDetailException()
    {
    }

    public IPDetailException(string? message)
        : base(message)
    {
    }

    public IPDetailException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }

    protected IPDetailException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
