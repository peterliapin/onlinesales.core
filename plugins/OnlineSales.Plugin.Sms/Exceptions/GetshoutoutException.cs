// <copyright file="GetshoutoutException.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

namespace OnlineSales.Plugin.Sms.Exceptions;

public class GetshoutoutException : Exception
{
    public GetshoutoutException()
    {
    }

    public GetshoutoutException(string? message)
        : base(message)
    {
    }

    public GetshoutoutException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }
}