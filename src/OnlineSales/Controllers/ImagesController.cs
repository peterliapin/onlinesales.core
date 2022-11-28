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
using Nest;
using OnlineSales.Data;
using OnlineSales.DTOs;
using OnlineSales.Entities;
using Quartz.Util;
using YamlDotNet.Core.Tokens;

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
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> Post([FromForm] ImageCreateDto imageCreateDto)
        {
            if (!ModelState.IsValid)
            {
                return UnprocessableEntity(ModelState);
            }

            var provider = new FileExtensionContentTypeProvider();

            string incomingFileName = imageCreateDto.Image!.FileName;
            string incomingFileExtension = Path.GetExtension(imageCreateDto.Image!.FileName);
            long incomingFileSize = imageCreateDto.Image!.Length; // bytes
            string? incomingFileMimeType = string.Empty;

            if (!provider.TryGetContentType(incomingFileName, out incomingFileMimeType))
            {
                return BadRequest("MIME of the file not identified.");
            }

            using var fileStream = imageCreateDto.Image.OpenReadStream();
            byte[] imageInBytes = new byte[incomingFileSize];
            fileStream.Read(imageInBytes, 0, (int)imageCreateDto.Image.Length);

            Image uploadedImage = new ()
            {
                Name = incomingFileName,
                Size = incomingFileSize,
                Data = imageInBytes,
                MimeType = incomingFileMimeType!,
                ScopeUId = imageCreateDto.ScopeId,
                Extension = incomingFileExtension,
            };

            await apiDbContext.Images!.AddAsync(uploadedImage);
            await apiDbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { scopeId = imageCreateDto.ScopeId, fileName = incomingFileName }, imageCreateDto);
        }

        [Route("{scopeId}/{fileName}")]
        [ResponseCache(CacheProfileName = "ImageResponse")]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
    }
}