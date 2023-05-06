// <copyright file="MediaController.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using OnlineSales.Data;
using OnlineSales.DTOs;
using OnlineSales.Entities;

namespace OnlineSales.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class MediaController : ControllerBase
    {
        private readonly PgDbContext pgDbContext;

        public MediaController(PgDbContext pgDbContext)
        {
            this.pgDbContext = pgDbContext;
        }

        [HttpPost]
        [ProducesResponseType(typeof(MediaDetailsDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> Post([FromForm] ImageCreateDto imageCreateDto)
        {
            var provider = new FileExtensionContentTypeProvider();

            var incomingFileName = imageCreateDto.Image!.FileName;
            var incomingFileExtension = Path.GetExtension(imageCreateDto.Image!.FileName);
            var incomingFileSize = imageCreateDto.Image!.Length; // bytes
            var incomingFileMimeType = string.Empty;

            if (!provider.TryGetContentType(incomingFileName, out incomingFileMimeType))
            {
                ModelState.AddModelError("FileName", "Unsupported MIME type");

                throw new InvalidModelStateException(ModelState);
            }

            using var fileStream = imageCreateDto.Image.OpenReadStream();
            var imageInBytes = new byte[incomingFileSize];
            fileStream.Read(imageInBytes, 0, (int)imageCreateDto.Image.Length);

            var scopeAndFileExists = from i in pgDbContext!.Media!
                                     where i.ScopeUid == imageCreateDto.ScopeUid.Trim() && i.Name == incomingFileName
                                     select i;
            if (scopeAndFileExists.Any())
            {
                var uploadedImage = scopeAndFileExists!.FirstOrDefault();
                uploadedImage!.Data = imageInBytes;
                uploadedImage!.Size = incomingFileSize;

                pgDbContext.Media!.Update(uploadedImage);
            }
            else
            {
                Media uploadedMedia = new()
                {
                    Name = incomingFileName,
                    Size = incomingFileSize,
                    Data = imageInBytes,
                    MimeType = incomingFileMimeType!,
                    ScopeUid = imageCreateDto.ScopeUid.Trim(),
                    Extension = incomingFileExtension,
                };

                await pgDbContext.Media!.AddAsync(uploadedMedia);
            }

            await pgDbContext.SaveChangesAsync();

            Log.Information("Request scheme {0}", HttpContext.Request.Scheme);
            Log.Information("Request host {0}", HttpContext.Request.Host.Value);

            var fileData = new MediaDetailsDto()
            {
                Location = Path.Combine(HttpContext.Request.Path, imageCreateDto.ScopeUid, incomingFileName).Replace("\\", "/"),
            };
            return CreatedAtAction(nameof(Get), new { scopeUid = imageCreateDto.ScopeUid, fileName = incomingFileName }, fileData);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("{scopeUid}/{fileName}")]
        [ResponseCache(CacheProfileName = "ImageResponse")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> Get([Required] string scopeUid, [Required] string fileName)
        {
            var uploadedImageData = await (from upi in pgDbContext!.Media! where upi.ScopeUid == scopeUid && upi.Name == fileName select upi).FirstOrDefaultAsync();

            if (uploadedImageData == null)
            {
                throw new EntityNotFoundException(nameof(Media), $"{scopeUid}/{fileName}");
            }

            return File(uploadedImageData!.Data, uploadedImageData.MimeType, fileName);
        }
    }
}