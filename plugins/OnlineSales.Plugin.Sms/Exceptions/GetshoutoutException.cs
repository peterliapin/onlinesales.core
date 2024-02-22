// <copyright file="GetshoutoutException.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Runtime.Serialization;

namespace OnlineSales.Plugin.Sms.Exceptions;

[Serializable]
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