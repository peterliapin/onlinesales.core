// <copyright file="IpDetailsException.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Runtime.Serialization;

namespace OnlineSales.Exceptions
{
    [Serializable]
    public class IpDetailsException : Exception
    {
        public IpDetailsException()
        {
        }

        public IpDetailsException(string? message)
            : base(message)
        {
        }

        public IpDetailsException(string? message, Exception? innerException)
            : base(message, innerException)
        {
        }

        protected IpDetailsException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
