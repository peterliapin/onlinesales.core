// <copyright file="BaseController.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Web;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSales.Data;
using OnlineSales.Entities;
using OnlineSales.ErrorHandling;
using OnlineSales.Infrastructure;

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
        protected readonly IErrorMessageGenerator errorMessageGenerator;

        public BaseController(ApiDbContext dbContext, IMapper mapper, IErrorMessageGenerator errorMessageGenerator)
        {
            this.dbContext = dbContext;
            this.dbSet = dbContext.Set<T>();
            this.mapper = mapper;
            this.errorMessageGenerator = errorMessageGenerator;
        }

        // GET api/{entity}s/5
        [HttpGet("{id}")]
        // [EnableQuery]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public virtual async Task<ActionResult<T>> GetOne(int id)
        {
            var result = await (from p in this.dbSet
                                where p.Id == id
                                select p).FirstOrDefaultAsync();

            if (result == null)
            {
                return CreateNotFoundMessageResult(id);
            }

            return Ok(result);
        }

        // POST api/{entity}s
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public virtual async Task<ActionResult<T>> Post([FromBody] TC value)
        {
            if (!ModelState.IsValid)
            {
                return CreateValidationErrorMessageResult();
            }

            var newValue = mapper.Map<T>(value);
            var result = await dbSet.AddAsync(newValue);
            await dbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetOne), new { id = result.Entity.Id }, value);
        }

        // PUT api/posts/5
        [HttpPatch("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public virtual async Task<ActionResult<T>> Patch(int id, [FromBody] TU value)
        {
            if (!ModelState.IsValid)
            {
                return CreateValidationErrorMessageResult();
            }

            var existingEntity = await (from p in this.dbSet
                                        where p.Id == id
                                        select p).FirstOrDefaultAsync();

            if (existingEntity == null)
            {
                return CreateNotFoundMessageResult(id);
            }

            mapper.Map(value, existingEntity);
            await dbContext.SaveChangesAsync();

            return Ok();
        }

        // DELETE api/posts/5
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public virtual async Task<ActionResult> Delete(int id)
        {
            var existingEntity = await (from p in this.dbSet
                                        where p.Id == id
                                        select p).FirstOrDefaultAsync();

            if (existingEntity == null)
            {
                return CreateNotFoundMessageResult(id);
            }

            dbContext.Remove(existingEntity);

            await dbContext.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public virtual async Task<ActionResult<List<T>>> Get([FromQuery] IDictionary<string, string>? parameters)
        {
            var query = this.dbSet!.AsQueryable<T>();
            if (!this.Request.QueryString.HasValue)
            {
                return Ok(await query.ToListAsync());
            }

            var queryCommands = this.Request.QueryString.ToString().Substring(1).Split('&').Select(s => HttpUtility.UrlDecode(s)).ToArray(); // Removing '?' character, split by '&'

            query = QueryBuilder<T>.ReadIntoQuery(query, queryCommands, out var selectExists);
            if (selectExists)
            {
                var selectResult = await QueryBuilder<T>.ExecuteSelectExpression(query, queryCommands);
                return Ok(selectResult);
            }

            var result = await query!.ToListAsync();
            return Ok(result);
        }

        protected ActionResult CreateNotFoundMessageResult(int id)
        {
            return errorMessageGenerator.CreateNotFoundResponce(InnerErrorCodes.Status404.IdNotFound, typeof(T).Name, id.ToString());
        }

        protected ActionResult CreateValidationErrorMessageResult()
        {
            return errorMessageGenerator.CreateBadRequestResponce(this, InnerErrorCodes.Status400.ValidationErrors);
        }
    }
}