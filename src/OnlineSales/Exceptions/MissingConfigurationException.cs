// <copyright file="MissingConfigurationException.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Runtime.Serialization;

namespace OnlineSales.Exceptions;

[Serializable]
public class MissingConfigurationException : Exception
{
    public MissingConfigurationException()
    {
    }

    public MissingConfigurationException(string? message)
        : base(message)
    {
    }

    public MissingConfigurationException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }
}