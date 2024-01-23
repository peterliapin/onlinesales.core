// <copyright file="SyncEmailLogTaskException.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Runtime.Serialization;

namespace OnlineSales.Exceptions
{
    [Serializable]
    public class SyncEmailLogTaskException : Exception
    {
        public SyncEmailLogTaskException()
        {
        }

        public SyncEmailLogTaskException(string? message)
            : base(message)
        {
        }

        public SyncEmailLogTaskException(string? message, Exception? innerException)
            : base(message, innerException)
        {
        }
    }
}
