// <copyright file="MissingIdentityException.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Runtime.Serialization;

namespace OnlineSales.Plugin.AzureAD.Exceptions;

[Serializable]
public class MissingIdentityException : Exception
{
    public MissingIdentityException()
    {
    }

    public MissingIdentityException(string? message)
        : base(message)
    {
    }

    public MissingIdentityException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }

    protected MissingIdentityException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
