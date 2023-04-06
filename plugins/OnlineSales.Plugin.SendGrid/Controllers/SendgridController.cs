// <copyright file="SendgridController.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Collections.Immutable;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using OnlineSales.Data;
using OnlineSales.Entities;
using OnlineSales.Helpers;
using OnlineSales.Interfaces;
using OnlineSales.Plugin.SendGrid.Data;
using OnlineSales.Plugin.SendGrid.DTOs;
using OnlineSales.Plugin.SendGrid.Entities;
using OnlineSales.Plugin.SendGrid.Exceptions;
using SendGrid.Helpers.EventWebhook;
using Serilog;

namespace OnlineSales.Plugin.SendGrid.Controllers;

[Route("api/[controller]")]
public class SendgridController : ControllerBase
{
    private static readonly int BatchSize = 500;    

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

    private async Task<Dictionary<Contact, List<T>>> CreateNonExistedContacts<T>(IEnumerable<IGrouping<string, T>> emailAndRecords)
    {
        var contactRecords = new Dictionary<Contact, List<T>>();
        var nonExistedContacts = new List<Contact>();

        int position = 0;

        while (position < emailAndRecords.Count())
        {
            var batch = emailAndRecords.Skip(position).Take(BatchSize);
            var existedContacts = dbContext.Contacts!.Where(c => batch.Select(b => b.Key).Contains(c.Email)).ToDictionary(c => c.Email, c => c);
            foreach (var b in batch)
            {
                var isExists = existedContacts.TryGetValue(b.Key, out var contact);
                if (!isExists)
                {
                    contact = new Contact() { Email = b.Key };
                    nonExistedContacts.Add(contact);
                }

                contactRecords[contact!] = b.ToList();
            }

            position += BatchSize;
        }

        await contactService.SaveAsync(nonExistedContacts);

        return contactRecords;
    }

    private async Task AddEventRecords<T>(Dictionary<Contact, List<T>> contactsAndRecords)
    {
        int position = 0;
        while (position < contactsAndRecords.Count)
        {
            var batch = contactsAndRecords.Skip(position).Take(BatchSize);
            var existingRecords = dbContext.SendgridEvents!.Where(e => batch.Select(b => b.Key).Contains(e.Contact)).ToList();
            foreach (var contactAndRecords in batch)
            {
                foreach (var record in contactAndRecords.Value)
                {
                    var sgevent = Convert<T>(record, contactAndRecords.Key);
                    var existingRecord = existingRecords.FirstOrDefault(e => e.Contact!.Email == sgevent.Contact!.Email && e.Event == sgevent.Event && e.CreatedAt == sgevent.CreatedAt);
                    if (existingRecord == null)
                    {
                        await dbContext.SendgridEvents!.AddAsync(sgevent);
                    }
                }
            }

            position += BatchSize;
        }
    }

    private async Task<ActionResult> AddEvents<T>(List<T> records)
        where T : EmailDto
    {
        try
        {
            var emailRecords = records.GroupBy(r => r.Email); 

            var contactRecords = await CreateNonExistedContacts(emailRecords);

            await AddEventRecords(contactRecords);

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
            throw new SendGridApiException("Cannot create SendgridEvent");
        }
    }

    private SendgridEvent Convert(MessageEventDto me, Contact contact)
    {
        return new SendgridEvent()
        {
            CreatedAt = GetDateTime(me.Processed),
            Event = GetEvent(me),
            MessageId = me.SendGridMessageId,
            Reason = me.Reason,
            Ip = me.OriginatingIp,
            Contact = contact,
        };
    }

    private SendgridEvent Convert(WebhookEventDto we, Contact contact)
    {
        return new SendgridEvent()
        {
            CreatedAt = DateTimeHelper.GetDateTime(we.Timestamp),
            Event = GetEvent(we),
            MessageId = we.SendGridMessageId,
            Reason = we.Reason,
            Ip = we.Ip,
            Contact = contact,
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
