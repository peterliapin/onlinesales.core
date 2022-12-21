// <copyright file="PostsController.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineSales.Data;
using OnlineSales.DTOs;
using OnlineSales.Entities;
using OnlineSales.ErrorHandling;
using OnlineSales.Interfaces;

namespace OnlineSales.Controllers;

[Authorize]
[Route("api/[controller]")]
public class PostsController : BaseController<Post, PostCreateDto, PostUpdateDto>
{
    public PostsController(ApiDbContext dbContext, IMapper mapper, IErrorMessageGenerator errorMessageGenerator)
        : base(dbContext, mapper, errorMessageGenerator)
    {
    }
}