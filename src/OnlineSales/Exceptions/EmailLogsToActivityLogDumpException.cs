// <copyright file="EmailLogsToActivityLogDumpException.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Runtime.Serialization;

namespace OnlineSales.Exceptions
{
    [Serializable]
    public class EmailLogsToActivityLogDumpException : Exception
    {
        public EmailLogsToActivityLogDumpException()
        {
        }

        public EmailLogsToActivityLogDumpException(string? message)
            : base(message)
        {
        }

        public EmailLogsToActivityLogDumpException(string? message, Exception? innerException)
            : base(message, innerException)
        {
        }

        protected EmailLogsToActivityLogDumpException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
