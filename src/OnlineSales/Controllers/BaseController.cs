// <copyright file="BaseController.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
using OnlineSales.Data;
using OnlineSales.Entities;

namespace OnlineSales.Controllers
{
    public class BaseController<T, TC, TU> : ControllerBase
        where T : BaseEntity, new()
        where TC : class
        where TU : class
    {
        protected readonly DbSet<T> dbSet;
        protected readonly DbContext dbContext;
        protected readonly IMapper mapper;

        public BaseController(ApiDbContext dbContext, IMapper mapper)
        {
            this.dbContext = dbContext;
            this.dbSet = dbContext.Set<T>();
            this.mapper = mapper;
        }

        // GET api/{entity}s/
        [HttpGet]
        [EnableQuery(PageSize = 10)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public virtual ActionResult<IQueryable<T>> GetAll()
        {
            var result = this.dbSet!.AsQueryable<T>();
            
            return Ok(result);
        }

        // GET api/{entity}s/5
        [HttpGet("{id}")]
        [EnableQuery]
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
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<T>> Post([FromBody] TC value)
        {
            if (!ModelState.IsValid)
            {
                return UnprocessableEntity(ModelState);
            }

            var newValue = mapper.Map<T>(value);

            newValue.CreatedAt = DateTime.UtcNow;
            newValue.CreatedByIP = GetClientIP();
            newValue.CreatedByUserAgent = GetUserAgent();

            var result = await dbSet.AddAsync(newValue);

            await dbContext.SaveChangesAsync();
             
            return CreatedAtAction(nameof(GetOne), new { id = result.Entity.Id }, value);
        }

        // PUT api/posts/5
        [HttpPatch("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<T>> Patch(int id, [FromBody] TU value)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return UnprocessableEntity(ModelState);
                }

                var existingEntity = await (from p in this.dbSet
                                            where p.Id == id
                                            select p).FirstOrDefaultAsync();

                if (existingEntity == null)
                {
                    return NotFound();
                }

                mapper.Map(value, existingEntity);

                existingEntity.UpdatedAt = DateTime.UtcNow;
                existingEntity.UpdatedByIP = GetClientIP();
                existingEntity.UpdatedByUserAgent = GetUserAgent();

                await dbContext.SaveChangesAsync();

                return Ok();
            }
            catch (Exception ex)
            {
                return Problem(
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: ex.Message);
            }
        }

        // DELETE api/posts/5
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> Delete(int id)
        {
            var existingEntity = await (from p in this.dbSet
                                       where p.Id == id
                                       select p).FirstOrDefaultAsync();

            if (existingEntity == null)
            {
                return NotFound();
            }

            dbContext.Remove(existingEntity);

            await dbContext.SaveChangesAsync();

            return NoContent();
        }

        private string? GetClientIP()
        {
            return HttpContext?.Connection?.RemoteIpAddress?.ToString();
        }

        private string? GetUserAgent()
        {
            return HttpContext?.Request?.Headers[HeaderNames.UserAgent];
        }
    }
}