// <copyright file="EmailController.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel.DataAnnotations;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineSales.Data;
using OnlineSales.DTOs;
using OnlineSales.Interfaces;

namespace OnlineSales.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class EmailController : ControllerBase
    {
        private readonly IEmailVerifyService emailVerifyService;
        private readonly IMapper mapper;

        public EmailController(IEmailVerifyService emailVerifyService, IMapper mapper)
        {
            this.emailVerifyService = emailVerifyService;
            this.mapper = mapper;
        }

        [HttpGet("verify/{email}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> Verify([EmailAddress] string email)
        {
            var resultedDomainData = await this.emailVerifyService.Verify(email);

            var resultConverted = this.mapper.Map<EmailVerifyDetailsDto>(resultedDomainData);

            resultConverted.EmailAddress = email;

            return this.Ok(resultConverted);
        }
    }
}
