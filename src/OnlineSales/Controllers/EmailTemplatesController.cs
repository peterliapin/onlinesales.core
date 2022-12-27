// <copyright file="EmailTemplatesController.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineSales.Data;
using OnlineSales.DTOs;
using OnlineSales.Entities;

namespace OnlineSales.Controllers;

[Authorize]
[Route("api/[controller]")]
public class EmailTemplatesController : BaseFKController<EmailTemplate, EmailTemplateCreateDto, EmailTemplateUpdateDto, EmailGroup>
{
    public EmailTemplatesController(ApiDbContext dbContext, IMapper mapper)
    : base(dbContext, mapper)
    {
    }

    protected override (int, string) GetFKId(EmailTemplateCreateDto item)
    {
        return (item.GroupId, "GroupId");
    }

    protected override (int?, string) GetFKId(EmailTemplateUpdateDto item)
    {
        return (item.GroupId, "GroupId");
    }
}