// <copyright file="PostsController.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>
using Microsoft.AspNetCore.Mvc;
using OnlineSales.Data;
using OnlineSales.DTOs;
using OnlineSales.Models;

namespace OnlineSales.Controllers;

[Route("api/[controller]")]
public class PostsController : Controller
{
    private readonly ApiDbContext dbContext;

    public PostsController(ApiDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    // GET: api/posts
    [HttpGet]
    public IEnumerable<Post> Get()
    {
        var posts = this.dbContext.Posts!.ToList<Post>();
        return posts;
    }

    // GET api/posts/5
    [HttpGet("{id}")]
    public Post Get(string id)
    {
        var post = (from p in this.dbContext.Posts
                    where p.Id == id
                    select p).First();

        return post;
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