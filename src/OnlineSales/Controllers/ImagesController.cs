// <copyright file="ImagesController.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.Net.Http.Headers;
using OnlineSales.Data;
using OnlineSales.DTOs;
using OnlineSales.Entities;
using Quartz.Util;

namespace OnlineSales.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImagesController : ControllerBase
    {
        private readonly ApiDbContext apiDbContext;

        public ImagesController(ApiDbContext apiDbContext)
        {
            this.apiDbContext = apiDbContext;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> Post([FromForm] ImageCreateDto imageCreateDto)
        {
            var provider = new FileExtensionContentTypeProvider();

            string incomingFileName = imageCreateDto.Image!.FileName;
            string incomingFileExtension = Path.GetExtension(imageCreateDto.Image!.FileName);
            long incomingFileSize = imageCreateDto.Image!.Length; // bytes
            string? incomingFileMimeType = string.Empty;

            provider.TryGetContentType(incomingFileName, out incomingFileMimeType);

            using var fileStream = imageCreateDto.Image.OpenReadStream();
            byte[] imageInBytes = new byte[incomingFileSize];
            fileStream.Read(imageInBytes, 0, (int)imageCreateDto.Image.Length);

            string baseUrl = $"{Request.Scheme}://{Request.Host.Value}{Request.Path}";

            Image uploadedImage = new ()
            {
                Name = incomingFileName,
                Size = incomingFileSize,
                Data = imageInBytes,
                MimeType = incomingFileMimeType!,
                ScopeUId = imageCreateDto.ScopeId,
                Extension = incomingFileExtension,
                CreatedAt = DateTime.UtcNow,
                CreatedByIP = GetClientIP(),
                CreatedByUserAgent = GetUserAgent(),
            };

            await apiDbContext.Images!.AddAsync(uploadedImage);

            await apiDbContext.SaveChangesAsync();

            return Ok(new { ImageUrl = baseUrl + "/" + imageCreateDto.ScopeId + "/" + incomingFileName });
        }

        [Route("{scopeId}/{fileName}")]
        [ResponseCache(VaryByHeader = "User-Agent", Duration = 120)]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> Get(string scopeId, string fileName)
        {
            if (scopeId.IsNullOrWhiteSpace())
            {
                return BadRequest("Scope is invalid");
            }

            if (fileName.IsNullOrWhiteSpace())
            {
                return BadRequest("File Name is invalid");
            }

            var uploadedImageData = await (from upi in apiDbContext!.Images! where upi.ScopeUId == scopeId && upi.Name == fileName select upi).FirstOrDefaultAsync();

            if (uploadedImageData == null)
            {
                return BadRequest("Requested file not found");
            }

            return File(uploadedImageData!.Data, uploadedImageData.MimeType, fileName); 
        }

        private string? GetClientIP()
        {
            return HttpContext?.Connection?.RemoteIpAddress?.ToString();
        }

        private string? GetUserAgent()
        {
            return HttpContext?.Request?.Headers[HeaderNames.UserAgent];
        }
    }
}