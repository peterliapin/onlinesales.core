// <copyright file="WaveServiceController.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OnlineSales.DTOs;
using OnlineSales.Interfaces;
using OnlineSales.Plugin.WaveService.Services;
using OnlineSales.Services;

namespace OnlineSales.Plugin.WaveService.Controllers
{
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class WaveServiceController : ControllerBase
    {
        private readonly ResourceService resourceService;
        private readonly IEmailFromTemplateService emailService;
        // private readonly IEmailVerifyService emailVerifyService;

        public WaveServiceController(ResourceService resourceService, IEmailFromTemplateService emailService, IEmailVerifyService emailVerifyService)
        {
            this.resourceService = resourceService;
            this.emailService = emailService;
            // this.emailVerifyService = emailVerifyService;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> SendEmail(string email, string fileName)
        {
            // Create local mx test service and use it
            // var domain = await emailVerifyService.Verify(email);
            // if (domain.CatchAll is not true)
            // {
            //     return NotFound(email);
            // }

            var fileResName = Path
                .GetFileNameWithoutExtension(fileName)
                .Replace("-", "_")
                .Replace(".", "_")
                .ToLower();

            var file = resourceService.GetFile(fileResName, "_pdf");

            if (file == null)
            {
                return BadRequest("File not found");
            }

            var description = resourceService.GetDescription(fileResName) ?? string.Empty;

            if (string.IsNullOrEmpty(description))
            {
                return BadRequest("Description for file not found");
            }

            var attachment = new AttachmentDto
            {
                File = file,
                FileName = fileName,
            };

            var templateArguments = new Dictionary<string, string>()
            {
                { "CaseName", description },
            };

            await emailService.SendAsync("WA_Email_SendFile", "ru", new string[] { email }, templateArguments, new List<AttachmentDto> { attachment });

            // Send notify to Telegram WA Group on success

            return Ok();
        }
    }
}
