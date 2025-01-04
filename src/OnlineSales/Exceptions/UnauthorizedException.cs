// <copyright file="UnauthorizedException.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

namespace OnlineSales.Exceptions;

public class UnauthorizedException : Exception
{
    public UnauthorizedException()
        : base("Failed to login")
    {
    }
}