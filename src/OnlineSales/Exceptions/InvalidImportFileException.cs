// <copyright file="InvalidImportFileException.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Runtime.Serialization;

namespace OnlineSales.Exceptions
{
    public class InvalidImportFileException : Exception
    {
        public InvalidImportFileException()
        {
        }

        public InvalidImportFileException(string? message)
            : base(message)
        {
        }

        public InvalidImportFileException(string? message, Exception? innerException)
            : base(message, innerException)
        {
        }

        protected InvalidImportFileException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
