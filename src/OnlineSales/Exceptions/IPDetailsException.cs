// <copyright file="IPDetailsException.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Runtime.Serialization;

namespace OnlineSales.Exceptions
{
    [Serializable]
    public class IPDetailsException : Exception
    {
        public IPDetailsException()
        {
        }

        public IPDetailsException(string? message)
            : base(message)
        {
        }

        public IPDetailsException(string? message, Exception? innerException)
            : base(message, innerException)
        {
        }

        protected IPDetailsException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
