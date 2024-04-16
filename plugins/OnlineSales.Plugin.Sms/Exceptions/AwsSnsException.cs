// <copyright file="AwsSnsException.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Runtime.Serialization;

namespace OnlineSales.Plugin.Sms.Exceptions
{
    [Serializable]
    public class AwsSnsException : Exception
    {
        public AwsSnsException()
        {
        }

        public AwsSnsException(string? message)
            : base(message)
        {
        }

        public AwsSnsException(string? message, Exception? innerException)
            : base(message, innerException)
        {
        }

        protected AwsSnsException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}