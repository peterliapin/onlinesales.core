// <copyright file="VersionController.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Diagnostics;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using OnlineSales.Interfaces;

namespace OnlineSales.Controllers
{
    [Route("api/[controller]")]
    public class VersionController : ControllerBaseEH
    {
        private readonly IHttpContextHelper? httpContextHelper;

        public VersionController(IHttpContextHelper? httpContextHelper)
        {
            this.httpContextHelper = httpContextHelper;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult Get()
        {
            try
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
                return Ok(
                    new
                    {
                        Version = fileVersionInfo.ProductVersion!,
                        IP = httpContextHelper!.IpAddress,
                        IPv4 = httpContextHelper!.IpAddressV4,
                        IPv6 = httpContextHelper!.IpAddressV6,
                    });
            }
            catch (Exception ex)
            {
                return errorHandler.CreateInternalServerErrorResponce(ex.Message);
            }
        }
    }
}