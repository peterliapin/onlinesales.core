// <copyright file="ImapAccountsController.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OnlineSales.Configuration;
using OnlineSales.Controllers;
using OnlineSales.Data;
using OnlineSales.Plugin.EmailSync.Data;
using OnlineSales.Plugin.EmailSync.DTOs;
using OnlineSales.Plugin.EmailSync.Entities;

namespace OnlineSales.Plugin.EmailSync.Controllers;

[Authorize]
[Route("api/[controller]")]
public class ImapAccountsController : BaseController<ImapAccount, ImapAccountCreateDto, ImapAccountUpdateDto, ImapAccountDetailsDto>
{
    public ImapAccountsController(EmailSyncDbContext dbContext, IMapper mapper, IOptions<ApiSettingsConfig> apiSettingsConfig, EsDbContext esDbContext)
    : base(dbContext, mapper, apiSettingsConfig, esDbContext)
    {
    }
}
