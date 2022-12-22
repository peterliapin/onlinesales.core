// <copyright file="EmailTemplatesController.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineSales.Data;
using OnlineSales.DTOs;
using OnlineSales.Entities;
using OnlineSales.ErrorHandling;

namespace OnlineSales.Controllers;

[Authorize]
[Route("api/[controller]")]
public class EmailTemplatesController : BaseFKController<EmailTemplate, EmailTemplateCreateDto, EmailTemplateUpdateDto, EmailGroup>
{
    public EmailTemplatesController(ApiDbContext dbContext, IMapper mapper, IErrorMessageGenerator errorMessageGenerator)
    : base(dbContext, mapper, errorMessageGenerator)
    {
    }

    protected override int GetFKId(EmailTemplateCreateDto item)
    {
        return item.GroupId;
    }

    protected override int? GetFKId(EmailTemplateUpdateDto item)
    {
        return item.GroupId;
    }
}
