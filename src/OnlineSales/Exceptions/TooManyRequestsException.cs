// <copyright file="TooManyRequestsException.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

namespace OnlineSales.Exceptions;

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
}