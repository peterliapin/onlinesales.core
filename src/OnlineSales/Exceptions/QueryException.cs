// <copyright file="QueryException.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>
using System.Runtime.Serialization;
using Microsoft.AspNetCore.WebUtilities;

namespace OnlineSales.Exceptions
{
    [Serializable]
    public class QueryException : Exception
    {
        public QueryException(string failedCommand, string errorDescription)
        {
            FailedCommands.Add(new KeyValuePair<string, string>(failedCommand, errorDescription));
        }

        public QueryException(IEnumerable<QueryException> innerExceptions)
        {
            FailedCommands = innerExceptions.SelectMany(e => e.FailedCommands).ToList();
        }

        protected QueryException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public List<KeyValuePair<string, string>> FailedCommands { get; init; } = new List<KeyValuePair<string, string>>();
    }
}
