// <copyright file="NotifyLkException.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Runtime.Serialization;

namespace OnlineSales.Plugin.Sms.Exceptions
{
    [Serializable]
    public class NotifyLkException : Exception
    {
        public NotifyLkException()
        {
        }

        public NotifyLkException(string? message)
            : base(message)
        {
        }

        public NotifyLkException(string? message, Exception? innerException)
            : base(message, innerException)
        {
        }

        protected NotifyLkException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}