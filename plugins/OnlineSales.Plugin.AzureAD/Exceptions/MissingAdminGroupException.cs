// <copyright file="MissingAdminGroupException.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>
using System.Runtime.Serialization;

namespace OnlineSales.Plugin.AzureAD.Exceptions
{
    [Serializable]
    public class MissingAdminGroupException : Exception
    {
        public MissingAdminGroupException()
        {
        }

        public MissingAdminGroupException(string? message)
            : base(message)
        {
        }

        public MissingAdminGroupException(string? message, Exception? innerException)
            : base(message, innerException)
        {
        }

        protected MissingAdminGroupException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
