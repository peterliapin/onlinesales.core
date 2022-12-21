// <copyright file="BaseController.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;
using AutoMapper;
using AutoMapper.Internal;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
using Microsoft.OData.ModelBuilder;
using OnlineSales.Data;
using OnlineSales.Entities;
using OnlineSales.Infrastructure;
using Quartz;

namespace OnlineSales.Controllers
{
    public class BaseController<T, TC, TU> : ControllerBaseEH
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

        // GET api/{entity}s/5
        [HttpGet("{id}")]
        // [EnableQuery]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public virtual async Task<ActionResult<T>> GetOne(int id)
        {
            try
            {
                var result = await (from p in this.dbSet
                                    where p.Id == id
                                    select p).FirstOrDefaultAsync();

                if (result == null)
                {
                    return errorHandler.CreateNotFoundResponce(CreateNotFoundMessage<T>(id));
                }

                return Ok(result);
            }
            catch (Exception e)
            {
                return errorHandler.CreateInternalServerErrorResponce(e.Message);
            }
        }

        // POST api/{entity}s
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public virtual async Task<ActionResult<T>> Post([FromBody] TC value)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return errorHandler.CreateBadRequestResponce();
                }

                var newValue = mapper.Map<T>(value);
                var result = await dbSet.AddAsync(newValue);
                await dbContext.SaveChangesAsync();

                return CreatedAtAction(nameof(GetOne), new { id = result.Entity.Id }, value);
            }
            catch (Exception e)
            {
                return errorHandler.CreateInternalServerErrorResponce(e.Message);
            }
        }

        // PUT api/posts/5
        [HttpPatch("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public virtual async Task<ActionResult<T>> Patch(int id, [FromBody] TU value)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return errorHandler.CreateBadRequestResponce();
                }

                var existingEntity = await (from p in this.dbSet
                                            where p.Id == id
                                            select p).FirstOrDefaultAsync();

                if (existingEntity == null)
                {
                    return errorHandler.CreateUnprocessableEntityResponce(CreateNotFoundMessage<T>(id));
                }

                mapper.Map(value, existingEntity);
                await dbContext.SaveChangesAsync();

                return Ok();
            }
            catch (Exception e)
            {
                return errorHandler.CreateInternalServerErrorResponce(e.Message);
            }
        }

        // DELETE api/posts/5
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public virtual async Task<ActionResult> Delete(int id)
        {
            try
            {
                var existingEntity = await (from p in this.dbSet
                                            where p.Id == id
                                            select p).FirstOrDefaultAsync();

                if (existingEntity == null)
                {
                    return errorHandler.CreateUnprocessableEntityResponce(CreateNotFoundMessage<T>(id));
                }

                dbContext.Remove(existingEntity);

                await dbContext.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception e)
            {
                return errorHandler.CreateInternalServerErrorResponce(e.Message);
            }
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public virtual async Task<ActionResult> Get([FromQuery] IDictionary<string, string>? parameters)
        {
            try
            {
                var queryCommands = this.Request.QueryString.ToString().Substring(1).Split('&').Select(s => HttpUtility.UrlDecode(s)).ToArray(); // Removing '?' character, split by '&'
                var query = this.dbSet!.AsQueryable<T>();

                query = QueryBuilder<T>.ReadIntoQuery(query, queryCommands, out var selectExists);
                if (selectExists)
                {
                    var selectResult = await QueryBuilder<T>.ExecuteSelectExpression(query, queryCommands);
                    return Ok(selectResult);
                }

                var result = await query!.ToArrayAsync();
                return Ok(result);
            }
            catch (Exception e)
            {
                return errorHandler.CreateInternalServerErrorResponce(e.Message);
            }
        }

        protected string CreateNotFoundMessage<TEntity>(int id)
        {
            // return JsonConvert.SerializeObject(new { Id = new string[] { string.Format("The Id field with value = {0} is not found", id) } }, Formatting.Indented);
            return string.Format("The {0} with Id = {1} is not found", typeof(TEntity).FullName, id);
        }
    }
}