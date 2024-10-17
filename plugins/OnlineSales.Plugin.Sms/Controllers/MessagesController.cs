// <copyright file="MessagesController.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OnlineSales.Plugin.Sms.Data;
using OnlineSales.Plugin.Sms.DTOs;
using OnlineSales.Plugin.Sms.Entities;
using Serilog;

namespace OnlineSales.Plugin.Sms.Controllers;

[Route("api/messages")]
public class MessagesController : MessagesControllerBase
{
    private readonly ISmsService smsService;

    public MessagesController(SmsDbContext dbContext, ISmsService smsService)
        : base(dbContext)
    {
        this.smsService = smsService;
    }

    [HttpPost]
    [Route("sms")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> SendSms(
        [FromBody] SmsDetailsDto smsDetails,
        [FromHeader(Name = "Authentication")] string accessToken)
    {
        return await SendMessage(accessToken, smsDetails.Recipient, smsDetails.Message);
    }

    protected override string GetSender(string recipient)
    {
        return smsService.GetSender(recipient);
    }

    protected override async Task SendMessage(string recipient, string message)
    {
        await smsService.SendAsync(recipient, message);
    }
}