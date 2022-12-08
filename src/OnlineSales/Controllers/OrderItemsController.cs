// <copyright file="OrderItemsController.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using OnlineSales.Data;
using OnlineSales.DTOs;
using OnlineSales.Entities;

namespace OnlineSales.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderItemsController : BaseController<OrderItem, OrderItemCreateDto, OrderItemUpdateDto>
    {
        public OrderItemsController(ApiDbContext dbContext, IMapper mapper)
            : base(dbContext, mapper)
        {
        }
    }
}
