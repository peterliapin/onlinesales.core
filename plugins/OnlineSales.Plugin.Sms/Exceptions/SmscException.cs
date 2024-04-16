// <copyright file="SmscException.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Runtime.Serialization;

namespace OnlineSales.Plugin.Sms.Exceptions
{
    [Serializable]
    public class SmscException : Exception
    {
        public SmscException()
        {
        }

        public SmscException(string? message)
            : base(message)
        {
        }

        public SmscException(string? message, Exception? innerException)
            : base(message, innerException)
        {
        }

        protected SmscException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}