// <copyright file="VersionController.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Diagnostics;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using OnlineSales.ErrorHandling;

namespace OnlineSales.Controllers
{
    [Route("api/[controller]")]
    public class VersionController : ControllerBase
    {
        protected readonly IErrorMessageGenerator errorMessageGenerator;

        public VersionController(IErrorMessageGenerator errorMessageGenerator)
        {
            this.errorMessageGenerator = errorMessageGenerator;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult Get()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            return Ok(new { Version = fileVersionInfo.ProductVersion! });
        }
    }
}
