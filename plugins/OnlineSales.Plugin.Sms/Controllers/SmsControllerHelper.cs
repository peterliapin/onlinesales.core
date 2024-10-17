// <copyright file="SmsControllerHelper.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using Microsoft.AspNetCore.Mvc.ModelBinding;
using OnlineSales.Exceptions;
using OnlineSales.Plugin.Sms.Data;
using OnlineSales.Plugin.Sms.Entities;
using PhoneNumbers;

namespace OnlineSales.Plugin.Sms.Controllers;

internal class SmsControllerHelper
{
    private readonly PhoneNumberUtil phoneNumberUtil = PhoneNumberUtil.GetInstance();
    private readonly SmsDbContext dbContext;

    public SmsControllerHelper(SmsDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public static void CheckAuthentication(string accessToken)
    {
        if (accessToken == null || accessToken.Replace("Bearer ", string.Empty) != SmsPlugin.Configuration.SmsAccessKey)
        {
            throw new UnauthorizedAccessException();
        }
    }

    public string GetRecipient(string phone, ModelStateDictionary modelState)
    {
        var recipient = string.Empty;
        try
        {
            var phoneNumber = phoneNumberUtil.Parse(phone, string.Empty);

            recipient = phoneNumberUtil.Format(phoneNumber, PhoneNumberFormat.E164);
        }
        catch (NumberParseException npex)
        {
            modelState.AddModelError(npex.ErrorType.ToString(), npex.Message);
        }

        if (!modelState.IsValid)
        {
            throw new InvalidModelStateException(modelState);
        }

        return recipient;
    }

    public async Task<SmsLog> AddLog(string recipient, string sender, string message)
    {
        var smsLog = new SmsLog
        {
            Sender = sender,
            Recipient = recipient,
            Message = message,
            Status = SmsLog.SmsStatus.NotSent,
        };

        dbContext.SmsLogs!.Add(smsLog);
        await dbContext.SaveChangesAsync();

        return smsLog;
    }

    public async Task UpdateLogStatus(SmsLog smsLog, SmsLog.SmsStatus status)
    {
        smsLog.Status = status;
        await dbContext.SaveChangesAsync();
    }
}
