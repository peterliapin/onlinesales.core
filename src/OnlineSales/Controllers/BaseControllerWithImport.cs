// <copyright file="BaseControllerWithImport.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OnlineSales.Configuration;
using OnlineSales.Data;
using OnlineSales.Entities;

namespace OnlineSales.Controllers;

public class BaseControllerWithImport<T, TC, TU, TD, TI> : BaseController<T, TC, TU, TD>
    where T : BaseEntity, new()
    where TC : class
    where TU : class
    where TD : class
    where TI : class
{
    public BaseControllerWithImport(ApiDbContext dbContext, IMapper mapper, IOptions<ApiSettingsConfig> apiSettingsConfig)
        : base(dbContext, mapper, apiSettingsConfig)
    {
    }

    [HttpPost]
    [Route("import")]
    public virtual async Task<ActionResult> Import([FromBody] List<TI> records)
    {
        // TODO: Add import logic here

        await Task.CompletedTask;

        return Ok();
    }
}

