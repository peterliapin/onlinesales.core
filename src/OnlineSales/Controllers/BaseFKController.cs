// <copyright file="BaseFKController.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSales.Data;
using OnlineSales.Entities;

namespace OnlineSales.Controllers
{
    public abstract class BaseFKController<T, TC, TU, TFK> : BaseController<T, TC, TU>
        where T : BaseEntity, new()
        where TC : class
        where TU : class
        where TFK : BaseEntity
    {
        protected readonly DbSet<TFK> dbFKSet;

        protected BaseFKController(ApiDbContext dbContext, IMapper mapper)
            : base(dbContext, mapper)
        {
            this.dbFKSet = dbContext.Set<TFK>();
        }

        // POST api/{entity}s
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public override async Task<ActionResult<T>> Post([FromBody] TC value)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return errorHandler.CreateBadRequestResponce();
                }

                var existFKItem = await (from fk in this.dbFKSet
                                         where fk.Id == GetFKId(value)
                                         select fk).FirstOrDefaultAsync();

                if (existFKItem == null)
                {
                    return errorHandler.CreateUnprocessableEntityResponce(CreateNotFoundMessage<TFK>(GetFKId(value)));
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

        [HttpPatch("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public override async Task<ActionResult<T>> Patch(int id, [FromBody] TU value)
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

                var fkid = GetFKId(value);
                if (fkid != null)
                {
                    var existFKItem = await (from fk in this.dbFKSet
                                             where fk.Id == fkid
                                             select fk).FirstOrDefaultAsync();

                    if (existFKItem == null)
                    {
                        return errorHandler.CreateUnprocessableEntityResponce(CreateNotFoundMessage<TFK>(fkid.Value));
                    }
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

        protected abstract int GetFKId(TC item);

        protected abstract int? GetFKId(TU item);
    }
}