// <copyright file="EmailController.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel.DataAnnotations;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OnlineSales.Data;
using OnlineSales.DTOs;
using OnlineSales.Interfaces;

namespace OnlineSales.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmailController : ControllerBase
    {
        private readonly ApiDbContext apiDbContext;
        private readonly IEmailVerifyService emailVerifyService;
        private readonly IMapper mapper;

        public EmailController(ApiDbContext apiDbContext, IEmailVerifyService emailVerifyService, IMapper mapper)
        {
            this.apiDbContext = apiDbContext;
            this.emailVerifyService = emailVerifyService;
            this.mapper = mapper;
        }

        [HttpGet("verify/{email}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> Verify([EmailAddress]string email)
        {
            var resultedDomainData = await emailVerifyService.Validate(email);

            var resultConverted = mapper.Map<EmailVerifyDetailsDto>(resultedDomainData);

            resultConverted.EmailAddress = email;

            return Ok(resultConverted);
        }
    }
}
