// <copyright file="ImagesController.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using OnlineSales.Data;
using OnlineSales.DTOs;
using OnlineSales.Entities;
using Quartz.Util;

namespace OnlineSales.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class ImagesController : ControllerBaseEH
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
            try
            {
                if (!ModelState.IsValid)
                {
                    return errorHandler.CreateBadRequestResponce();
                }

                var provider = new FileExtensionContentTypeProvider();

                string incomingFileName = imageCreateDto.Image!.FileName;
                string incomingFileExtension = Path.GetExtension(imageCreateDto.Image!.FileName);
                long incomingFileSize = imageCreateDto.Image!.Length; // bytes
                string? incomingFileMimeType = string.Empty;

                if (!provider.TryGetContentType(incomingFileName, out incomingFileMimeType))
                {
                    return errorHandler.CreateUnprocessableEntityResponce("MIME of the file not identified.");
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
                    { "location", $"{Path.Combine(this.HttpContext.Request.Path, imageCreateDto.ScopeUid, incomingFileName).Replace("\\", "/")}" },
                };
                return CreatedAtAction(nameof(Get), new { scopeUid = imageCreateDto.ScopeUid, fileName = incomingFileName }, fileData);
            }
            catch (Exception ex)
            {
                return errorHandler.CreateInternalServerErrorResponce(ex.Message);
            }
        }

        [Route("{scopeUid}/{fileName}")]
        [ResponseCache(CacheProfileName = "ImageResponse")]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> Get(string scopeUid, string fileName)
        {
            try
            {
                if (scopeUid.IsNullOrWhiteSpace())
                {
                    return errorHandler.CreateBadRequestResponce("Scope is invalid");
                }

                if (fileName.IsNullOrWhiteSpace())
                {
                    return errorHandler.CreateBadRequestResponce("File Name is invalid");
                }

                var uploadedImageData = await (from upi in apiDbContext!.Images! where upi.ScopeUid == scopeUid && upi.Name == fileName select upi).FirstOrDefaultAsync();

                if (uploadedImageData == null)
                {
                    return errorHandler.CreateNotFoundResponce(string.Format("Requested file with filename = {0} not found", fileName));
                }

                return File(uploadedImageData!.Data, uploadedImageData.MimeType, fileName);
            }
            catch (Exception ex)
            {
                return errorHandler.CreateInternalServerErrorResponce(ex.Message);
            }
        }
    }
}