// <copyright file="SendgridController.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Elasticsearch.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Nest;
using Newtonsoft.Json;
using OnlineSales.Data;
using OnlineSales.DTOs;
using OnlineSales.Entities;
using OnlineSales.Helpers;
using OnlineSales.Interfaces;
using OnlineSales.Plugin.SendGrid.Data;
using OnlineSales.Plugin.SendGrid.DTOs;
using OnlineSales.Plugin.SendGrid.Entities;
using OnlineSales.Plugin.SendGrid.Exceptions;
using SendGrid;
using SendGrid.Helpers.EventWebhook;
using Serilog;
using Serilog.Events;

namespace OnlineSales.Plugin.SendGrid.Controllers;

[Route("api/[controller]")]
public class SendgridController : ControllerBase
{
    private static readonly DateTime UnixTimestampZeroDate = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private readonly SendgridDbContext dbContext;

    private readonly IContactService contactService;

    public SendgridController(SendgridDbContext dbContext, EsDbContext esDbContext, IConfiguration configuration, IContactService contactService)
    {
        this.dbContext = dbContext;
        this.contactService = contactService;
    }

    [HttpPost]
    [Authorize]
    [Route("import")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Import([FromBody] List<MessageEventDto> records)
    {
        return await AddEvents(records);
    }

    [HttpPost]
    [AllowAnonymous]
    [Route("webhook")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> AddMessageEvents()
    {
        var signature = Request.Headers[RequestValidator.SIGNATURE_HEADER].First();
        var timestamp = Request.Headers[RequestValidator.TIMESTAMP_HEADER].First();
        var body = await GetRequestBody(HttpContext.Request.Body);

        if (signature == null || timestamp == null || !Verify(body, signature, timestamp))
        {
            throw new SendGridApiException("Webhook event can't be verified");
        }

        var records = JsonConvert.DeserializeObject<List<WebhookEventDto>>(body);

        if (records == null)
        {
            throw new SendGridApiException("Webhook event body can't be deserialized");
        }

        return await AddEvents(records);
    }

    private async Task<ActionResult> AddEvents<T>(List<T> records)
        where T : EmailDto
    {
        try
        {
            foreach (var r in records)
            {
                var contact = dbContext.Contacts!.Where(c => c.Email == r.Email).FirstOrDefault();
                if (contact == null)
                {
                    contact = new Contact() { Email = r.Email, };
                    await contactService.SaveAsync(contact);
                    var sgevent = Convert(r, contact);
                    await dbContext.SendgridEvents!.AddAsync(sgevent);
                }
                else
                {
                    var sgevent = Convert<T>(r, contact);
                    var existingEvent = dbContext.SendgridEvents!.Where(e => e.ContactId == sgevent.ContactId && e.Event == sgevent.Event && e.CreatedAt == sgevent.CreatedAt).FirstOrDefault();
                    if (existingEvent == null)
                    {
                        await dbContext.SendgridEvents!.AddAsync(sgevent);
                    }
                }
            }

            await dbContext.SaveChangesAsync();

            return Ok();
        }
        catch (Exception e)
        {
            Log.Error(e.Message);
            throw;
        }
    }

    private SendgridEvent Convert<T>(T record, Contact contact)
    {
        if (record is MessageEventDto me)
        {
            return Convert(me, contact);
        }
        else if (record is WebhookEventDto we)
        {
            return Convert(we, contact);
        }
        else
        {
            throw new SendGridApiException("Cannot create ActivityLogDto");
        }
    }

    private SendgridEvent Convert(MessageEventDto me, Contact contact)
    {
        return new SendgridEvent()
        {
            CreatedAt = GetDateTime(me.Processed),
            Event = GetEvent(me),
            MessageId = me.Message_id,
            Reason = me.Reason,
            Ip = me.Originating_ip,
            ContactId = contact.Id,
        };
    }

    private SendgridEvent Convert(WebhookEventDto we, Contact contact)
    {
        return new SendgridEvent()
        {
            CreatedAt = GetDateTime(we.Timestamp),
            Event = GetEvent(we),
            MessageId = we.Sg_Message_Id,
            Reason = we.Reason,
            Ip = we.Ip,
            ContactId = contact.Id,
        };
    }

    private bool Verify(string payload, string signature, string timestamp)
    {
        try
        {
            if (SendGridPlugin.Configuration.SendGridApi.WebhookPublicKeys.Count > 0)
            {
                foreach (var key in SendGridPlugin.Configuration.SendGridApi.WebhookPublicKeys)
                {
                    var validator = new RequestValidator();
                    var ecPublicKey = validator.ConvertPublicKeyToECDSA(key);
                    if (validator.VerifySignature(ecPublicKey, payload, signature, timestamp))
                    {
                        return true;
                    }
                }

                return false;
            }
            else
            {
                return false;
            }
        }
        catch
        {
            return false;
        }
    }

    private async Task<string> GetRequestBody(Stream body)
    {
        using var reader = new StreamReader(body);
        return await reader.ReadToEndAsync();
    }

    private DateTime GetDateTime(double timestamp)
    {
        return UnixTimestampZeroDate.AddSeconds(timestamp).ToUniversalTime();
    }

    private DateTime GetDateTime(string processed)
    {
        DateTime res;
        try
        {
            res = DateTime.ParseExact(processed, "yyyy-MM-dd HH:mm:ss.fff", System.Globalization.CultureInfo.InvariantCulture).ToUniversalTime();
        }
        catch (Exception)
        {
            throw new SendGridApiException("Wrong date in import data. Date: " + processed);
        }

        return res;
    }

    private string GetEvent(MessageEventDto me)
    {
        if (me.Event == "drop")
        {
            return "dropped";
        }
        else if (me.Event == "bounce" && me.Type == "blocked")
        {
            return "blocked";
        }
        else
        {
            return me.Event;
        }
    }

    private string GetEvent(WebhookEventDto we)
    {
        if (we.Event == "bounce" && we.Type == "blocked")
        {
            return "blocked";
        }
        else
        {
            return we.Event;
        }
    }
}
