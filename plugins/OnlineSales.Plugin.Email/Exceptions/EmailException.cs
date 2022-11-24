// <copyright file="EmailException.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Runtime.Serialization;

namespace OnlineSales.Plugin.Email.Exceptions;

[Serializable]
public class EmailException : Exception
{
    public EmailException()
    {
    }

    public EmailException(string? message)
        : base(message)
    {
    }

    public EmailException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }

    protected EmailException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
