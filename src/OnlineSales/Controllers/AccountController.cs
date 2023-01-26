// <copyright file="AccountController.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OnlineSales.Configuration;
using OnlineSales.Data;
using OnlineSales.DTOs;
using OnlineSales.Entities;

namespace OnlineSales.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : BaseControllerWithImport<Account, AccountCreateDto, AccountUpdateDto, AccountDetailsDto, AccountImportDto>
    {
        public AccountController(ApiDbContext dbContext, IMapper mapper, IOptions<ApiSettingsConfig> apiSettingsConfig)
            : base(dbContext, mapper, apiSettingsConfig)
        {
        }
    }
}
