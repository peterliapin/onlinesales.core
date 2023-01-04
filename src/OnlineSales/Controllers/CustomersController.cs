// <copyright file="CustomersController.cs" company="WavePoint Co. Ltd.">
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

namespace OnlineSales.Controllers;

[Authorize]
[Route("api/[controller]")]
public class CustomersController : BaseControllerWithImport<Customer, CustomerCreateDto, CustomerUpdateDto, CustomerDetailsDto, CustomerImportDto>
{
    public CustomersController(ApiDbContext dbContext, IMapper mapper, IOptions<ApiSettingsConfig> apiSettingsConfig)
        : base(dbContext, mapper, apiSettingsConfig)
    {
    }
}