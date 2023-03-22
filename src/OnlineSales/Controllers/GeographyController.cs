// <copyright file="GeographyController.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using Microsoft.AspNetCore.Mvc;
using OnlineSales.Geography;
using OnlineSales.Helpers;

namespace OnlineSales.Controllers;

[Route("api/[controller]")]
public class ContinentsController : ControllerBase
{
    // GET api/сontinents/
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public virtual ActionResult<Dictionary<string, string>> GetAll()
    {
        var result = EnumHelper.GetEnumDescriptions<Continent>();

        return Ok(result);
    }
}

[Route("api/[controller]")]
public class CountriesController : ControllerBase
{
    // GET api/сontinents/
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public virtual ActionResult<Dictionary<string, string>> GetAll()
    {
        var result = EnumHelper.GetEnumDescriptions<Country>();

        return Ok(result);
    }
}