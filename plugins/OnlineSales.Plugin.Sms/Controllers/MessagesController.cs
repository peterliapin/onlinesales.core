// <copyright file="MessagesController.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using Microsoft.AspNetCore.Authorization;
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
    private readonly ISmsService smsService;
    private readonly PhoneNumberUtil phoneNumberUtil = PhoneNumberUtil.GetInstance();
    private readonly SmsDbContext dbContext;

    public MessagesController(SmsDbContext dbContext, ISmsService smsService)
    {
        this.dbContext = dbContext;
        this.smsService = smsService;
    }

    [HttpPost]
    [Route("sms")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> SendSms(
        [FromBody] SmsDetailsDto smsDetails,
        [FromHeader(Name = "Authentication")] string accessToken)
    {
        try
        {
            if (accessToken == null || accessToken.Replace("Bearer ", string.Empty) != SmsPlugin.Configuration.SmsAccessKey)
            {
                return new UnauthorizedResult();
            }

            var recipient = string.Empty;

            try
            {
                var phoneNumber = this.phoneNumberUtil.Parse(smsDetails.Recipient, string.Empty);

                recipient = this.phoneNumberUtil.Format(phoneNumber, PhoneNumberFormat.E164);
            }
            catch (NumberParseException npex)
            {
                this.ModelState.AddModelError(npex.ErrorType.ToString(), npex.Message);
            }

            if (!this.ModelState.IsValid)
            {
                throw new InvalidModelStateException(this.ModelState);
            }

            var smsLog = new SmsLog
            {
                Sender = this.smsService.GetSender(recipient),
                Recipient = smsDetails.Recipient,
                Message = smsDetails.Message,
                Status = Entities.SmsLog.SmsStatus.NotSent,
            };

            this.dbContext.SmsLogs!.Add(smsLog);
            await this.dbContext.SaveChangesAsync();

            await this.smsService.SendAsync(recipient, smsDetails.Message);

            smsLog.Status = SmsLog.SmsStatus.Sent;

            return this.Ok();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to send SMS message to {0}: {1}", smsDetails.Recipient, smsDetails.Message);

            throw;
        }
        finally
        {
            await this.dbContext.SaveChangesAsync();
        }
    }
}