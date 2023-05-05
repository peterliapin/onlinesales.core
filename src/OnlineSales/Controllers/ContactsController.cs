// <copyright file="ContactsController.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Security.Cryptography;
using System.Text;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OnlineSales.Configuration;
using OnlineSales.Data;
using OnlineSales.DTOs;
using OnlineSales.Entities;
using OnlineSales.Helpers;
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

        singleItem!.AvatarUrl = GravatarHelper.EmailToGravatarUrl(singleItem.Email);

        return this.Ok(singleItem!);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public override async Task<ActionResult<List<ContactDetailsDto>>> Get([FromQuery] string? query)
    {
        var returnedItems = (await base.Get(query)).Result;

        var items = (List<ContactDetailsDto>)((ObjectResult)returnedItems!).Value!;

        items.ForEach(c =>
        {
            c.AvatarUrl = GravatarHelper.EmailToGravatarUrl(c.Email);
        });

        return this.Ok(items);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public override async Task<ActionResult<ContactDetailsDto>> Post([FromBody] ContactCreateDto value)
    {
        var contact = this.mapper.Map<Contact>(value);

        await this.contactService.SaveAsync(contact);

        await this.dbContext.SaveChangesAsync();

        var returnedValue = this.mapper.Map<ContactDetailsDto>(contact);

        returnedValue.AvatarUrl = GravatarHelper.EmailToGravatarUrl(returnedValue.Email);

        return this.CreatedAtAction(nameof(GetOne), new { id = contact.Id }, returnedValue);
    }

    [HttpPatch("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public override async Task<ActionResult<ContactDetailsDto>> Patch(int id, [FromBody] ContactUpdateDto value)
    {
        var existingContact = (from contact in this.dbContext.Contacts where contact.Id == id select contact).FirstOrDefault();

        if (existingContact == null)
        {
            throw new EntityNotFoundException("Contact", id.ToString());
        }

        this.mapper.Map(value, existingContact);

        await this.contactService.SaveAsync(existingContact);

        await this.dbContext.SaveChangesAsync();

        var returnedValue = this.mapper.Map<ContactDetailsDto>(existingContact);

        returnedValue.AvatarUrl = GravatarHelper.EmailToGravatarUrl(returnedValue.Email);

        return this.Ok(returnedValue);
    }

    protected override async Task SaveRangeAsync(List<Contact> newRecords)
    {
        await this.contactService.SaveRangeAsync(newRecords);
    }
}