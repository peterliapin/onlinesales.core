// <copyright file="UnknownCountryCodeException.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

namespace OnlineSales.Plugin.Sms.Exceptions
{
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
    }
}