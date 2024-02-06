// <copyright file="UnauthorizedException.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Runtime.Serialization;

namespace OnlineSales.Exceptions;

[Serializable]
public class UnauthorizedException : Exception
{
    public UnauthorizedException()
        : base("Failed to login")
    {
    }

    protected UnauthorizedException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}