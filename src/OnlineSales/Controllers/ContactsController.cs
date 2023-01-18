// <copyright file="ContactsController.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

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

// [Authorize]
[Route("api/[controller]")]
public class ContactsController : BaseControllerWithImport<Contact, ContactCreateDto, ContactUpdateDto, ContactDetailsDto, ContactImportDto>
{
    private readonly IContactService contactService;

    public ContactsController(ApiDbContext dbContext, IMapper mapper, IOptions<ApiSettingsConfig> apiSettingsConfig, IContactService contactService)
        : base(dbContext, mapper, apiSettingsConfig)
    {
        this.contactService = contactService;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public override async Task<ActionResult<ContactDetailsDto>> Post([FromBody] ContactCreateDto value)
    {
        var contact = mapper.Map<Contact>(value);

        var savedContact = await contactService.AddContact(contact);

        var returnedValue = mapper.Map<ContactDetailsDto>(savedContact);

        return CreatedAtAction(nameof(GetOne), new { id = savedContact.Id }, returnedValue);
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

        var updatedContact = await contactService.UpdateContact(existingContact);

        var returnedValue = mapper.Map<ContactDetailsDto>(updatedContact);

        return Ok(returnedValue);
    }
 
    protected override Task SaveBatchChangesAsync(List<Contact> importingRecords)
    {
        var newItems = contactService.EnrichWithDomainId(importingRecords);
        
        return base.SaveBatchChangesAsync(newItems);
    }
}