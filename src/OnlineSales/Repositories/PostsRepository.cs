// <copyright file="PostsRepository.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>
using OnlineSales.Data;
using OnlineSales.Models;

namespace OnlineSales.Repositories;

public class PostsRepository : IRepository<Post>
{
    private readonly ApiDbContext dbContext;

    public PostsRepository(ApiDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public void Add(Post instance)
    {
        throw new NotImplementedException();
    }

    public void Delete(string id)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<Post> GetAll()
    {
        return this.dbContext.Posts!.ToList();
    }

    public Post GetById(string id)
    {
        throw new NotImplementedException();
    }

    public void Save()
    {
        throw new NotImplementedException();
    }

    public void Update(Post instance)
    {
        throw new NotImplementedException();
    }
}

