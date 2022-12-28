// <copyright file="QueryException.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>
using System.Runtime.Serialization;

namespace OnlineSales.Exceptions
{
    [Serializable]
    public class QueryException : Exception
    {
        public QueryException()
        {
        }

        public QueryException(string? message)
            : base(message)
        {
        }

        public QueryException(string? message, Exception? innerException)
            : base(message, innerException)
        {
        }

        protected QueryException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
