// <copyright file="SendGridApiException.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

namespace OnlineSales.Plugin.SendGrid.Exceptions;

public class SendGridApiException : Exception
{
    public SendGridApiException(string message)
        : base(message)
    {
    }
}

