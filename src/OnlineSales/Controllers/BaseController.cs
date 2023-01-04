// <copyright file="BaseController.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Web;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OnlineSales.Configuration;
using OnlineSales.Data;
using OnlineSales.Entities;
using OnlineSales.Infrastructure;

namespace OnlineSales.Controllers
{
    public class BaseController<T, TC, TU, TD> : ControllerBase
        where T : BaseEntity, new()
        where TC : class
        where TU : class
        where TD : class
    {
        protected readonly DbSet<T> dbSet;  
        protected readonly ApiDbContext dbContext;
        protected readonly IMapper mapper;
        private readonly IOptions<ApiSettingsConfig> apiSettingsConfig;

        public BaseController(ApiDbContext dbContext, IMapper mapper, IOptions<ApiSettingsConfig> apiSettingsConfig)
        {
            this.dbContext = dbContext;
            this.dbSet = dbContext.Set<T>();
            this.mapper = mapper;
            this.apiSettingsConfig = apiSettingsConfig;
        }

        // GET api/{entity}s/5
        [HttpGet("{id}")]
        // [EnableQuery]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public virtual async Task<ActionResult<TD>> GetOne(int id)
        {
            var result = await FindOrThrowNotFound(id);

            var resultConverted = mapper.Map<TD>(result);

            return Ok(resultConverted);
        }

        // POST api/{entity}s
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]        
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public virtual async Task<ActionResult<TD>> Post([FromBody] TC value)
        {
            var newValue = mapper.Map<T>(value);
            var result = await dbSet.AddAsync(newValue);
            await dbContext.SaveChangesAsync();

            var resultsToClient = mapper.Map<TD>(newValue);

            return CreatedAtAction(nameof(GetOne), new { id = result.Entity.Id }, resultsToClient);
        }

        // PUT api/posts/5
        [HttpPatch("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public virtual async Task<ActionResult<TD>> Patch(int id, [FromBody] TU value)
        {
            var existingEntity = await FindOrThrowNotFound(id);

            mapper.Map(value, existingEntity);
            await dbContext.SaveChangesAsync();

            var resultsToClient = mapper.Map<TD>(existingEntity);

            return Ok(resultsToClient);
        }

        // DELETE api/posts/5
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public virtual async Task<ActionResult> Delete(int id)
        {
            var existingEntity = await FindOrThrowNotFound(id);

            dbContext.Remove(existingEntity);

            await dbContext.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public virtual async Task<ActionResult<List<TD>>> Get([FromQuery] IDictionary<string, string>? parameters)
        {
            int limit = apiSettingsConfig.Value.MaxListSize;
            var query = this.dbSet!.AsQueryable<T>();

            IList<T>? selectResult;

            if (this.Request.QueryString.HasValue)
            {
                this.Response.Headers.Add(ResponseHeaderNames.TotalCount, query.Count().ToString());
                return Ok(await query.Take(limit).ToListAsync()); // change this
            

            var queryCommands = this.Request.QueryString.ToString().Substring(1).Split('&').Select(s => HttpUtility.UrlDecode(s)).ToArray(); // Removing '?' character, split by '&'
            var countQueryCommands = queryCommands.Where(i => !i.ToLower().StartsWith("filter[limit]") && !i.ToLower().StartsWith("filter[skip]")).ToArray();

                var countQuery = QueryBuilder<T>.ReadIntoQuery(query, countQueryCommands);
                this.Response.Headers.Add(ResponseHeaderNames.TotalCount, countQuery.Count().ToString());

            var hasLimit = queryCommands.Where(i => i.ToLower().Contains("filter[limit]="));

            if (!hasLimit.Any())
            {
                string limitQueryString = "filter[limit]=" + limit.ToString();
                queryCommands = queryCommands.Append(limitQueryString).ToArray();
            }
            else
            {
                var requestedLimit = Convert.ToInt32(hasLimit !.FirstOrDefault() !.Split("=").Last());

                if (requestedLimit > limit)
                {
                    string message = "limit should be less than or equal : " + limit.ToString();
                    throw new InvalidModelStateException(message);
                }
            }

            query = QueryBuilder<T>.ReadIntoQuery(query, queryCommands, out var selectExists, out var anyValidCmds);
            if (!anyValidCmds && this.Request.QueryString.HasValue)
            {
                return Ok(Array.Empty<T>());
            }

                if (selectExists)
                {
                    selectResult = await QueryBuilder<T>.ExecuteSelectExpression(query, queryCommands);
                }
                else
                {
                    selectResult = await query!.ToListAsync();
                }
            }
            else
            {
                selectResult = await query.ToListAsync();
                this.Response.Headers.Add(ResponseHeaderNames.TotalCount, selectResult.Count.ToString());
            }

            var resultConverted = mapper.Map<List<TD>>(selectResult);
            return Ok(resultConverted);
        }

        protected async Task<T> FindOrThrowNotFound(int id)
        {
            var existingEntity = await (from p in this.dbSet
                                        where p.Id == id
                                        select p).FirstOrDefaultAsync();

            if (existingEntity == null)
            {
                throw new EntityNotFoundException(typeof(T).Name, id.ToString());
            }

            return existingEntity;
        }
    }
}