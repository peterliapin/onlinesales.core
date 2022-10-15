// <copyright file="PostsController.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>
using Microsoft.AspNetCore.Mvc;
using OnlineSales.Data;
using OnlineSales.DTOs;
using OnlineSales.Models;

namespace OnlineSales.Controllers;

[Route("api/[controller]")]
public class PostsController : BaseController<Post>
{
    public PostsController(ApiDbContext dbContext)
        : base(dbContext)
    {
    }

    // POST api/posts
    [HttpPost]
    public void Post([FromBody] Post value)
    {
        throw new NotSupportedException();
    }

    // PUT api/posts/5
    [HttpPut("{id}")]
    public void Put(int id, [FromBody] Post value)
    {
        throw new NotSupportedException();
    }

    // DELETE api/posts/5
    [HttpDelete("{id}")]
    public void Delete(int id)
    {
        throw new NotSupportedException();
    }
}