// <copyright file="MessagesController.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OnlineSales.Exceptions;
using OnlineSales.Plugin.Sms.Data;
using OnlineSales.Plugin.Sms.DTOs;
using OnlineSales.Plugin.Sms.Entities;
using PhoneNumbers;
using Serilog;

namespace OnlineSales.Plugin.Sms.Controllers;

[Route("api/messages")]
public class MessagesController : Controller
{
    protected readonly SmsDbContext dbContext;
    protected readonly ISmsService smsService;
    protected readonly PhoneNumberUtil phoneNumberUtil = PhoneNumberUtil.GetInstance();

    public MessagesController(SmsDbContext dbContext, ISmsService smsService)
    {
        this.dbContext = dbContext;
        this.smsService = smsService;
    }

    [HttpPost]
    [Route("sms")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> SendSms(
        [FromBody] SmsDetailsDto smsDetails,
        [FromHeader(Name = "Authentication")] string accessToken)
    {
        var smsLog = new Entities.SmsLog
        {
            Sender = string.Empty,
            Recipient = smsDetails.Recipient,
            Message = smsDetails.Message,
            Status = Entities.SmsLog.SmsStatus.Sent,
        };

        try
        {
            if (accessToken == null || accessToken.Replace("Bearer ", string.Empty) != SmsPlugin.Configuration.SmsAccessKey)
            {
                return new UnauthorizedResult();
            }

            string recipient = string.Empty;

            try
            {
                var phoneNumber = phoneNumberUtil.Parse(smsDetails.Recipient, string.Empty);

                recipient = phoneNumberUtil.Format(phoneNumber, PhoneNumberFormat.E164);
            }
            catch (NumberParseException npex)
            {
                ModelState.AddModelError(npex.ErrorType.ToString(), npex.Message);
            }

            if (!ModelState.IsValid)
            {
                throw new InvalidModelStateException(ModelState);
            }

            smsLog.Sender = smsService.GetSender(recipient);

            await smsService.SendAsync(recipient, smsDetails.Message);

            return Ok();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to send SMS message to {0}: {1}", smsDetails.Recipient, smsDetails.Message);

            smsLog.Status = SmsLog.SmsStatus.NotSent;

            return Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                title: ex.Message);
        }
        finally
        {
            dbContext.SmsLogs!.Add(smsLog);
            await dbContext.SaveChangesAsync();
        }
    }
}