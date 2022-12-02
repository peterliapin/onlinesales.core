// <copyright file="ImagesController.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
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
                return BadRequest(new { message = "MIME of the file not identified." });
            }

            using var fileStream = imageCreateDto.Image.OpenReadStream();
            byte[] imageInBytes = new byte[incomingFileSize];
            fileStream.Read(imageInBytes, 0, (int)imageCreateDto.Image.Length);
            
            var scopeAndFileExists = from i in apiDbContext!.Images!
                                     where i.ScopeUid == imageCreateDto.ScopeUid.Trim() && i.Name == incomingFileName
                                     select i;
            if (scopeAndFileExists.Any())
            {
                Image? uploadedImage = scopeAndFileExists!.FirstOrDefault();
                uploadedImage!.Data = imageInBytes;
                uploadedImage!.Size = incomingFileSize;

                apiDbContext.Images!.Update(uploadedImage);
            }
            else
            {
                Image uploadedImage = new ()
                {
                    Name = incomingFileName,
                    Size = incomingFileSize,
                    Data = imageInBytes,
                    MimeType = incomingFileMimeType!,
                    ScopeUid = imageCreateDto.ScopeUid.Trim(),
                    Extension = incomingFileExtension,
                };

                await apiDbContext.Images!.AddAsync(uploadedImage);
            }

            await apiDbContext.SaveChangesAsync();

            Log.Information("Request scheme {0}", this.HttpContext.Request.Scheme);
            Log.Information("Request host {0}", this.HttpContext.Request.Host.Value);

            var fileData = new Dictionary<string, string>()
            {
                { "location", $"{this.HttpContext.Request.Path}{imageCreateDto.ScopeUid}/{incomingFileName}" },
            };
            return CreatedAtAction(nameof(Get), new { scopeUid = imageCreateDto.ScopeUid, fileName = incomingFileName }, fileData);
        }

        [Route("{scopeUid}/{fileName}")]
        [ResponseCache(CacheProfileName = "ImageResponse")]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> Get(string scopeUid, string fileName)
        {
            if (scopeUid.IsNullOrWhiteSpace())
            {
                return BadRequest(new { message = "Scope is invalid" });
            }

            if (fileName.IsNullOrWhiteSpace())
            {
                return BadRequest(new { message = "File Name is invalid" });
            }

            var uploadedImageData = await (from upi in apiDbContext!.Images! where upi.ScopeUid == scopeUid && upi.Name == fileName select upi).FirstOrDefaultAsync();

            if (uploadedImageData == null)
            {
                return NotFound(new { message = "Requested file not found" });
            }

            return File(uploadedImageData!.Data, uploadedImageData.MimeType, fileName); 
        }
    }
}