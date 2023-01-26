// <copyright file="FailedToSendSmsException.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Runtime.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OnlineSales.Plugin.Sms.DTOs;

namespace OnlineSales.Exceptions;

public class FailedToSendSmsException : Exception
{
    public FailedToSendSmsException(SmsDetailsDto smsDetails)
        : base($"Failed to send SMS message to {smsDetails.Recipient}: {smsDetails.Message}")
    {
    }
}

