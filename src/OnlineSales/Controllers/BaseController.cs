// <copyright file="BaseController.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
using OnlineSales.Data;
using OnlineSales.Models;

namespace OnlineSales.Controllers
{
    public class BaseController<T> : Controller
        where T : BaseEntity
    {
        protected readonly DbSet<T> dbSet;
        protected readonly DbContext dbContext;

        public BaseController(ApiDbContext dbContext)
        {
            this.dbContext = dbContext;
            this.dbSet = dbContext.Set<T>();
        }

        // GET api/{entity}s/
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public virtual async Task<ActionResult<List<T>>> GetAll()
        {
            object? result = await this.dbSet!.ToListAsync();
            
            return Ok(result);
        }

        // GET api/{entity}s/5
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public virtual async Task<ActionResult<T>> GetOne(int id)
        {
            var result = await (from p in this.dbSet
                        where p.Id == id
                        select p).FirstOrDefaultAsync();

            if (result == null)
            {
                return NotFound();
            }

            return Ok(result);
        }

        // POST api/{entity}s
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<T>> Post([FromBody] T value)
        {
            value.CreatedAt = DateTime.UtcNow;
            value.CreatedByIP = GetClientIP();
            value.CreatedByUserAgent = GetUserAgent();

            if (!ModelState.IsValid)
            {
                return UnprocessableEntity(ModelState);
            }

            dbSet.Add(value);

            await dbContext.SaveChangesAsync();
             
            return CreatedAtAction(nameof(GetOne), new { id = value.Id }, value);
        }

        // PUT api/posts/5
        [HttpPut("{id}")]
        public async Task<ActionResult<T>> Put(int id, [FromBody] T value)
        {
            value.UpdatedAt = DateTime.UtcNow;
            value.UpdatedByIP = GetClientIP();
            value.UpdatedByUserAgent = GetUserAgent();

            ModelState.ClearValidationState(nameof(T));

            var existingValue = await (from p in this.dbSet
                                where p.Id == id
                                select p).FirstOrDefaultAsync();

            if (existingValue == null)
            {
                return NotFound();
            }

            if (!TryValidateModel(value, nameof(T)))
            {
                return UnprocessableEntity(ModelState);
            }

            // TODO: mapping fom value to existingValue should go here

            return Ok();
        }

        // DELETE api/posts/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
            throw new NotSupportedException();
        }

        private string GetClientIP()
        {
            return HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? string.Empty;
        }

        private string GetUserAgent()
        {
            return HttpContext?.Request?.Headers[HeaderNames.UserAgent] ?? string.Empty;
        }
    }
}