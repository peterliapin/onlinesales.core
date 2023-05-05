// <copyright file="BaseController.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Web;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Nest;
using OnlineSales.Configuration;
using OnlineSales.Data;
using OnlineSales.DataAnnotations;
using OnlineSales.Entities;
using OnlineSales.Infrastructure;

namespace OnlineSales.Controllers
{
    public class BaseController<T, TC, TU, TD> : ControllerBase
        where T : BaseEntityWithId, new()
        where TC : class
        where TU : class
        where TD : class
    {
        protected readonly DbSet<T> dbSet;
        protected readonly PgDbContext dbContext;
        protected readonly IMapper mapper;
        private readonly IOptions<ApiSettingsConfig> apiSettingsConfig;
        private readonly ElasticClient elasticClient;

        public BaseController(PgDbContext dbContext, IMapper mapper, IOptions<ApiSettingsConfig> apiSettingsConfig, EsDbContext esDbContext)
        {
            this.dbContext = dbContext;
            this.dbSet = dbContext.Set<T>();
            this.mapper = mapper;
            this.apiSettingsConfig = apiSettingsConfig;
            this.elasticClient = esDbContext.ElasticClient;
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
            var result = await this.FindOrThrowNotFound(id);

            var resultConverted = this.mapper.Map<TD>(result);

            return this.Ok(resultConverted);
        }

        // POST api/{entity}s
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public virtual async Task<ActionResult<TD>> Post([FromBody] TC value)
        {
            var newValue = this.mapper.Map<T>(value);
            var result = await this.dbSet.AddAsync(newValue);
            await this.dbContext.SaveChangesAsync();

            var resultsToClient = this.mapper.Map<TD>(newValue);

            return this.CreatedAtAction(nameof(GetOne), new { id = result.Entity.Id }, resultsToClient);
        }

        // PUT api/{entity}s/5
        [HttpPatch("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public virtual async Task<ActionResult<TD>> Patch(int id, [FromBody] TU value)
        {
            var existingEntity = await this.FindOrThrowNotFound(id);

            this.mapper.Map(value, existingEntity);
            await this.dbContext.SaveChangesAsync();

            var resultsToClient = this.mapper.Map<TD>(existingEntity);

            return this.Ok(resultsToClient);
        }

        // DELETE api/{entity}s/5
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public virtual async Task<ActionResult> Delete(int id)
        {
            var existingEntity = await this.FindOrThrowNotFound(id);

            this.dbContext.Remove(existingEntity);

            await this.dbContext.SaveChangesAsync();

            return this.NoContent();
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public virtual async Task<ActionResult<List<TD>>> Get([FromQuery] string? query)
        {
            var limit = this.apiSettingsConfig.Value.MaxListSize;

            var qp = this.BuildQueryProvider(limit);

            var result = await qp.GetResult();
            this.Response.Headers.Add(ResponseHeaderNames.TotalCount, result.TotalCount.ToString());
            this.Response.Headers.Add(ResponseHeaderNames.AccessControlExposeHeader, ResponseHeaderNames.TotalCount);
            return this.Ok(this.mapper.Map<List<TD>>(result.Records));
        }

        [HttpGet("export")]
        [Produces("text/csv", "text/json")]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public virtual async Task<ActionResult<List<TD>>> Export([FromQuery] string? query)
        {
            var qp = this.BuildQueryProvider(int.MaxValue);

            var result = await qp.GetResult();
            this.Response.Headers.Add(ResponseHeaderNames.TotalCount, result.TotalCount.ToString());
            this.Response.Headers.Add(ResponseHeaderNames.AccessControlExposeHeader, ResponseHeaderNames.TotalCount);

            return this.Ok(this.mapper.Map<List<TD>>(result.Records));
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

        private IQueryProvider<T> BuildQueryProvider(int maxLimitSize)
        {
            var queryCommands = this.Request.QueryString.HasValue ? HttpUtility.UrlDecode(this.Request.QueryString.ToString()).Substring(1).Split('&').ToArray() : new string[0];
            var parseData = new QueryParseData<T>(queryCommands, maxLimitSize);

            if (typeof(T).GetCustomAttributes(typeof(SupportsElasticAttribute), true).Any() && parseData.SearchData.Count > 0)
            {
                var indexPrefix = this.dbContext.Configuration.GetSection("Elastic:IndexPrefix").Get<string>();
                return new ESQueryProvider<T>(this.elasticClient, parseData, indexPrefix!);
            }
            else
            {
                return new DBQueryProvider<T>(this.dbSet!.AsQueryable<T>(), parseData);
            }
        }
    }
}