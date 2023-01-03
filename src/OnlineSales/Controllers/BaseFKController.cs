// <copyright file="BaseFKController.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OnlineSales.Configuration;
using OnlineSales.Data;
using OnlineSales.Entities;

namespace OnlineSales.Controllers
{
    public abstract class BaseFKController<T, TC, TU, TFK, TRE> : BaseController<T, TC, TU, TRE>
        where T : BaseEntity, new()
        where TC : class
        where TU : class
        where TFK : BaseEntity
        where TRE : class
    {
        protected readonly DbSet<TFK> dbFKSet;

        protected BaseFKController(ApiDbContext dbContext, IMapper mapper, IOptions<ApiSettingsConfig> apiSettingsConfig)
            : base(dbContext, mapper, apiSettingsConfig)
        {
            this.dbFKSet = dbContext.Set<TFK>();
        }

        // POST api/{entity}s
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public override async Task<ActionResult<TRE>> Post([FromBody] TC value)
        {
            var existFKItem = await (from fk in this.dbFKSet
                                     where fk.Id == GetFKId(value).Item1
                                     select fk).FirstOrDefaultAsync();

            if (existFKItem == null)
            {
                ModelState.AddModelError(GetFKId(value).Item2, "The referenced object was not found");

                throw new InvalidModelStateException(ModelState);
            }

            var newValue = mapper.Map<T>(value);
            var result = await dbSet.AddAsync(newValue);
            await dbContext.SaveChangesAsync();

            var returnValue = mapper.Map<TRE>(newValue);

            return CreatedAtAction(nameof(GetOne), new { id = result.Entity.Id }, returnValue);
        }

        [HttpPatch("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public override async Task<ActionResult<TRE>> Patch(int id, [FromBody] TU value)
        {
            var existingEntity = await FindOrThrowNotFound(id);

            var fkid = GetFKId(value);

            if (fkid.Item1 != null)
            {
                var existFKItem = await (from fk in this.dbFKSet
                                            where fk.Id == fkid.Item1
                                            select fk).FirstOrDefaultAsync();

                if (existFKItem == null)
                {
                    ModelState.AddModelError(fkid.Item2, "The referenced object was not found");

                    throw new InvalidModelStateException(ModelState);
                }
            }

            mapper.Map(value, existingEntity);
            await dbContext.SaveChangesAsync();

            return Ok();
        }

        protected abstract (int, string) GetFKId(TC item);

        protected abstract (int?, string) GetFKId(TU item);
    }
}