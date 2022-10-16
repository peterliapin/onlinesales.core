// <copyright file="CommentsController.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using OnlineSales.Data;
using OnlineSales.DTOs;
using OnlineSales.Models;

namespace OnlineSales.Controllers;

[Route("api/[controller]")]
public class CommentsController : BaseController<Comment, CommentCreateDto, CommentUpdateDto>
{
    public CommentsController(ApiDbContext dbContext, IMapper mapper)
        : base(dbContext, mapper)
    {
    }
}

