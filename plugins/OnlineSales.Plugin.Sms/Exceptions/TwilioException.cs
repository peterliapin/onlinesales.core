// <copyright file="TwilioException.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Runtime.Serialization;

namespace OnlineSales.Plugin.Sms.Exceptions
{
    [Serializable]
    public class TwilioException : Exception
    {
        public TwilioException()
        {
        }

        public TwilioException(string? message)
            : base(message)
        {
        }

        public TwilioException(string? message, Exception? innerException)
            : base(message, innerException)
        {
        }
    }
}