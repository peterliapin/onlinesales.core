// <copyright file="SmscException.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

namespace OnlineSales.Plugin.Sms.Exceptions
{
    public class SmscException : Exception
    {
        public SmscException()
        {
        }

        public SmscException(string? message)
            : base(message)
        {
        }

        public SmscException(string? message, Exception? innerException)
            : base(message, innerException)
        {
        }
    }
}