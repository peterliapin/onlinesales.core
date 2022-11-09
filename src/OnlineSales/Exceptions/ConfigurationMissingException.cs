// <copyright file="ConfigurationMissingException.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Runtime.Serialization;

namespace OnlineSales.Exceptions;

[Serializable]
public class ConfigurationMissingException : Exception
{
    public ConfigurationMissingException()
    {
    }

    public ConfigurationMissingException(string? message)
        : base(message)
    {
    }

    public ConfigurationMissingException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }

    protected ConfigurationMissingException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}