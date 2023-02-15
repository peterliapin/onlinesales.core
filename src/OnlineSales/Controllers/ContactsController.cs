// <copyright file="ContactsController.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Security.Cryptography;
using System.Text;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OnlineSales.Configuration;
using OnlineSales.Data;
using OnlineSales.DTOs;
using OnlineSales.Entities;
using OnlineSales.Interfaces;

namespace OnlineSales.Controllers;

[Authorize]
[Route("api/[controller]")]
public class ContactsController : BaseControllerWithImport<Contact, ContactCreateDto, ContactUpdateDto, ContactDetailsDto, ContactImportDto>
{
    private readonly IContactService contactService;

    public ContactsController(PgDbContext dbContext, IMapper mapper, IOptions<ApiSettingsConfig> apiSettingsConfig, IContactService contactService, EsDbContext esDbContext)
        : base(dbContext, mapper, apiSettingsConfig, esDbContext)
    {
        this.contactService = contactService;
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public override async Task<ActionResult<ContactDetailsDto>> GetOne(int id)
    {
        var returnedSingleItem = (await base.GetOne(id)).Result;

        var singleItem = (ContactDetailsDto)((ObjectResult)returnedSingleItem!).Value!;

        singleItem!.AvatarUrl = EmailToGravatarUrl(singleItem.Email);

        return Ok(singleItem!);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public override async Task<ActionResult<List<ContactDetailsDto>>> Get([FromQuery] IDictionary<string, string>? parameters)
    {
        var returnedItems = (await base.Get(parameters)).Result;

        var items = (List<ContactDetailsDto>)((ObjectResult)returnedItems!).Value!;

        items.ForEach(c =>
        {
            c.AvatarUrl = EmailToGravatarUrl(c.Email);
        });

        return Ok(items);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public override async Task<ActionResult<ContactDetailsDto>> Post([FromBody] ContactCreateDto value)
    {
        var contact = mapper.Map<Contact>(value);

        await contactService.SaveContact(contact);

        var returnedValue = mapper.Map<ContactDetailsDto>(contact);

        returnedValue.AvatarUrl = EmailToGravatarUrl(returnedValue.Email);

        return CreatedAtAction(nameof(GetOne), new { id = contact.Id }, returnedValue);
    }

    [HttpPatch("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public override async Task<ActionResult<ContactDetailsDto>> Patch(int id, [FromBody] ContactUpdateDto value)
    {
        var existingContact = (from contact in dbContext.Contacts where contact.Id == id select contact).FirstOrDefault();

        if (existingContact == null)
        {
            throw new EntityNotFoundException("Contact", id.ToString());
        }

        mapper.Map(value, existingContact);

        await contactService.SaveContact(existingContact);

        var returnedValue = mapper.Map<ContactDetailsDto>(existingContact);

        returnedValue.AvatarUrl = EmailToGravatarUrl(returnedValue.Email);

        return Ok(returnedValue);
    }

    protected override async Task SaveBatchChangesAsync(List<Contact> contacts)
    {
        await contactService.EnrichWithDomainId(contacts);

        await base.SaveBatchChangesAsync(contacts);
    }

    private static string EmailToGravatarUrl(string email)
    {
        byte[] emailBytes = Encoding.ASCII.GetBytes(email);
        byte[] emailHashCode = MD5.Create().ComputeHash(emailBytes);        

        return "https://www.gravatar.com/avatar/" + Convert.ToHexString(emailHashCode).ToLower() + "?size=48&d=mp";
    }
}