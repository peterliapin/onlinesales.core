// <copyright file="UnknownCountryCodeException.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Runtime.Serialization;

namespace OnlineSales.Plugin.Sms.Exceptions
{
    [Serializable]
    public class UnknownCountryCodeException : Exception
    {
        public UnknownCountryCodeException()
        {
        }

        public UnknownCountryCodeException(string? message)
            : base(message)
        {
        }

        public UnknownCountryCodeException(string? message, Exception? innerException)
            : base(message, innerException)
        {
        }

        protected UnknownCountryCodeException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}