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
public class OtpController : MessagesControllerBase
{
    private readonly IOtpService otpService;

    public OtpController(SmsDbContext dbContext, IOtpService otpService)
        : base(dbContext)
    {
        this.otpService = otpService;
    }

    [HttpPost]
    [Route("otp")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> SendOtp(
        [FromBody] OtpDetailsDto otpDetails,
        [FromHeader(Name = "Authentication")] string accessToken)
    {
        return await SendMessage(accessToken, otpDetails.Recipient, otpDetails.OtpCode);
    }

    protected override string GetSender(string recipient)
    {
        var sender = SmsPlugin.Configuration.OtpGateways.Telegram.SenderUserName;
        return string.IsNullOrEmpty(sender) || sender == "$OTPGATEWAYS__TELEGRAM__SENDERUSERNAME" ? "Telegram" : sender;
    }

    protected override async Task SendMessage(string recipient, string message)
    {
        await otpService.SendOtpAsync(recipient, message);
    }
}
