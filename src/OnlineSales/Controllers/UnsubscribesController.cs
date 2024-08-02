// <copyright file="UnsubscribesController.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineSales.Data;
using OnlineSales.DTOs;
using OnlineSales.Entities;
using OnlineSales.Infrastructure;

namespace OnlineSales.Controllers;

[Authorize(Roles = "Admin")]
[Route("api/[controller]")]
public class UnsubscribesController : BaseControllerWithImport<Unsubscribe, UnsubscribeDto, UnsubscribeDto, UnsubscribeDetailsDto, UnsubscribeImportDto>
{
    public UnsubscribesController(PgDbContext dbContext, IMapper mapper, EsDbContext esDbContext, QueryProviderFactory<Unsubscribe> queryProviderFactory)
        : base(dbContext, mapper, esDbContext, queryProviderFactory)
    {
    }
}