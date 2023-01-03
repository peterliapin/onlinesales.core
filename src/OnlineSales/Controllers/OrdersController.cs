// <copyright file="OrdersController.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineSales.Data;
using OnlineSales.DTOs;
using OnlineSales.Entities;

namespace OnlineSales.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class OrdersController : BaseControllerWithImport<Order, OrderCreateDto, OrderUpdateDto, OrderDetailsDto, OrderImportDto>
    {
        public OrdersController(ApiDbContext dbContext, IMapper mapper)
            : base(dbContext, mapper)
        {
        }
    }
}