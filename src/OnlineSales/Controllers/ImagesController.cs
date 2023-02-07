// <copyright file="ImagesController.cs" company="WavePoint Co. Ltd.">
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
    public class ImagesController : ControllerBase
    {
        private readonly PgDbContext pgDbContext;

        public ImagesController(PgDbContext pgDbContext)
        {
            this.pgDbContext = pgDbContext;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
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

            var scopeAndFileExists = from i in pgDbContext!.Images!
                                        where i.ScopeUid == imageCreateDto.ScopeUid.Trim() && i.Name == incomingFileName
                                        select i;
            if (scopeAndFileExists.Any())
            {
                var uploadedImage = scopeAndFileExists!.FirstOrDefault();
                uploadedImage!.Data = imageInBytes;
                uploadedImage!.Size = incomingFileSize;

                pgDbContext.Images!.Update(uploadedImage);
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

                await pgDbContext.Images!.AddAsync(uploadedImage);
            }

            await pgDbContext.SaveChangesAsync();

            Log.Information("Request scheme {0}", this.HttpContext.Request.Scheme);
            Log.Information("Request host {0}", this.HttpContext.Request.Host.Value);

            var fileData = new Dictionary<string, string>()
            {
                { "location", $"{Path.Combine(this.HttpContext.Request.Path, imageCreateDto.ScopeUid, incomingFileName).Replace("\\", "/")}" },
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
            var uploadedImageData = await (from upi in pgDbContext!.Images! where upi.ScopeUid == scopeUid && upi.Name == fileName select upi).FirstOrDefaultAsync();

            if (uploadedImageData == null)
            {
                throw new EntityNotFoundException(nameof(Image), $"{scopeUid}/{fileName}");
            }

            return File(uploadedImageData!.Data, uploadedImageData.MimeType, fileName);
        }
    }
}