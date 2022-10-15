// <copyright file="CommentsController.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>
using Microsoft.AspNetCore.Mvc;
using OnlineSales.Data;
using OnlineSales.Models;

namespace OnlineSales.Controllers;

[Route("api/[controller]")]
public class CommentsController : BaseController<Comment>
{
    public CommentsController(ApiDbContext dbContext)
        : base(dbContext)
    {
    }
}

