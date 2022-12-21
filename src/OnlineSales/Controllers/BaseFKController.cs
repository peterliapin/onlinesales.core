// <copyright file="BaseFKController.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
using Nest;
using Newtonsoft.Json;
using OnlineSales.Data;
using OnlineSales.Entities;
using OnlineSales.ErrorHandling;
using YamlDotNet.Core.Tokens;

namespace OnlineSales.Controllers
{
    public abstract class BaseFKController<T, TC, TU, TFK> : BaseController<T, TC, TU>
        where T : BaseEntity, new()
        where TC : class
        where TU : class
        where TFK : BaseEntity
    {
        protected readonly DbSet<TFK> dbFKSet;

        protected BaseFKController(ApiDbContext dbContext, IMapper mapper, IErrorMessageGenerator errorMessageGenerator)
            : base(dbContext, mapper, errorMessageGenerator)
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
            if (!ModelState.IsValid)
            {
                return CreateValidationErrorMessageResult();
            }

            var existFKItem = await (from fk in this.dbFKSet
                                        where fk.Id == GetFKId(value)
                                        select fk).FirstOrDefaultAsync();

            if (existFKItem == null)
            {
                return CreateUnprocessableEntityResult(GetFKId(value));
            }

            var newValue = mapper.Map<T>(value);
            var result = await dbSet.AddAsync(newValue);
            await dbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetOne), new { id = result.Entity.Id }, value);
        }

        [HttpPatch("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public override async Task<ActionResult<T>> Patch(int id, [FromBody] TU value)
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

            var fkid = GetFKId(value);
            if (fkid != null)
            {
                var existFKItem = await (from fk in this.dbFKSet
                                            where fk.Id == fkid
                                            select fk).FirstOrDefaultAsync();

                if (existFKItem == null)
                {
                    return CreateUnprocessableEntityResult(fkid.Value);
                }
            }

            mapper.Map(value, existingEntity);
            await dbContext.SaveChangesAsync();

            return Ok();
        }

        protected ActionResult CreateUnprocessableEntityResult(int fkId, string fkTypeName)
        {
            return errorMessageGenerator.CreateUnprocessableEntityResponce(this, InnerErrorCodes.Status422.FKIdNotFound, fkTypeName, fkId.ToString());
        }

        protected ActionResult CreateUnprocessableEntityResult(int fkId)
        {
            return CreateUnprocessableEntityResult(fkId, typeof(TFK).Name);
        }

        protected abstract int GetFKId(TC item);

        protected abstract int? GetFKId(TU item);
    }
}