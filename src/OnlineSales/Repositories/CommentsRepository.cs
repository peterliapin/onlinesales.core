// <copyright file="CommentsRepository.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>
using OnlineSales.Models;

namespace OnlineSales.Repositories;

public class CommentsRepository : IRepository<Comment>
{
    public CommentsRepository()
    {
    }

    public void Add(Comment instance)
    {
        throw new NotImplementedException();
    }

    public void Delete(string id)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<Comment> GetAll()
    {
        throw new NotImplementedException();
    }

    public Comment GetById(string id)
    {
        throw new NotImplementedException();
    }

    public void Save()
    {
        throw new NotImplementedException();
    }

    public void Update(Comment instance)
    {
        throw new NotImplementedException();
    }
}

