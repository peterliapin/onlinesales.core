// <copyright file="SyncEmailLogTaskException.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

namespace OnlineSales.Exceptions
{
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