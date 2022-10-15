// <copyright file="BaseController.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSales.Data;
using OnlineSales.Models;

namespace OnlineSales.Controllers
{
    public class BaseController<T> : Controller
        where T : BaseEntity
    {
        protected readonly DbSet<T> dbSet;

        public BaseController(ApiDbContext dbContext)
        {
            this.dbSet = dbContext.Set<T>();
        }

        // GET api/{entity}
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            object? result = await this.dbSet!.FindAsync();
            
            return this.Ok(result);
        }

        // GET api/{entity}/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOne(int id)
        {
            var result = await (from p in this.dbSet
                        where p.Id == id
                        select p).FirstAsync();

            return this.Ok(result);
        }
    }
}