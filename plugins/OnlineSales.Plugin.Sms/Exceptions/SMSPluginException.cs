// <copyright file="SMSPluginException.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

namespace OnlineSales.Plugin.Sms.Exceptions
{
    public class SmsPluginException : Exception
    {
        public SmsPluginException()
        {
        }

        public SmsPluginException(string? message)
            : base(message)
        {
        }

        public SmsPluginException(string? message, Exception? innerException)
            : base(message, innerException)
        {
        }
    }
}