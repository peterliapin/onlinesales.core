// <copyright file="UnsubscribesController.cs" company="WavePoint Co. Ltd.">
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
using OnlineSales.Infrastructure;
using OnlineSales.Interfaces;

namespace OnlineSales.Controllers;

[Authorize]
[Route("api/[controller]")]
public class UnsubscribesController : BaseControllerWithImport<Unsubscribe, UnsubscribeDto, UnsubscribeDto, UnsubscribeDetailsDto, UnsubscribeImportDto>
{
    public UnsubscribesController(PgDbContext dbContext, IMapper mapper, IDomainService domainService, EsDbContext esDbContext, QueryProviderFactory<Unsubscribe> queryFactory)
        : base(dbContext, mapper, esDbContext, queryFactory)
    {
    }
}