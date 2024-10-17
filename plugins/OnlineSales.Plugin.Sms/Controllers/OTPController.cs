// <copyright file="OTPController.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OnlineSales.Plugin.Sms.Data;
using OnlineSales.Plugin.Sms.DTOs;
using Serilog;

namespace OnlineSales.Plugin.Sms.Controllers;

[Route("api/otp")]
public class OtpController : Controller
{
    private readonly IOtpService otpService;
    private readonly SmsControllerHelper controllerHelper;

    public OtpController(SmsDbContext dbContext, IOtpService otpService)
    {
        this.otpService = otpService;

        controllerHelper = new SmsControllerHelper(dbContext);
    }

    [HttpPost]
    [Route("otp")]
    [AllowAnonymous] // @@ why?
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> SendSms(
        [FromBody] OtpDetailsDto otpDetails,
        [FromHeader(Name = "Authentication")] string accessToken)
    {
        try
        {
            SmsControllerHelper.CheckAuthentication(accessToken);
            var recipient = controllerHelper.GetRecipient(otpDetails.Recipient, ModelState);
            
            // @@var smsLog = await controllerHelper.AddLog(recipient, "OTP", otpDetails.OtpCode);

            await otpService.SendOtpAsync(recipient, otpDetails.OtpCode);

            // @@await controllerHelper.UpdateLogStatus(smsLog, SmsLog.SmsStatus.Sent);

            return Ok();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to send OTP code to {0}: {1}", otpDetails.Recipient, otpDetails.OtpCode);

            throw;
        }
    }
}
