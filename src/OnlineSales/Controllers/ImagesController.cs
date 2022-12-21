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
using OnlineSales.ErrorHandling;
using Quartz.Util;

namespace OnlineSales.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class ImagesController : ControllerBase
    {
        private readonly ApiDbContext apiDbContext;
        private readonly IErrorMessageGenerator errorMessageGenerator;

        public ImagesController(ApiDbContext apiDbContext, IErrorMessageGenerator errorMessageGenerator)
        {
            this.apiDbContext = apiDbContext;
            this.errorMessageGenerator = errorMessageGenerator;
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
                return errorMessageGenerator.CreateBadRequestResponce(InnerErrorCodes.Status400.ValidationErrors);
            }

            var provider = new FileExtensionContentTypeProvider();

            string incomingFileName = imageCreateDto.Image!.FileName;
            string incomingFileExtension = Path.GetExtension(imageCreateDto.Image!.FileName);
            long incomingFileSize = imageCreateDto.Image!.Length; // bytes
            string? incomingFileMimeType = string.Empty;

            if (!provider.TryGetContentType(incomingFileName, out incomingFileMimeType))
            {
                return errorMessageGenerator.CreateUnprocessableEntityResponce(InnerErrorCodes.Status422.MIMINotIdentified, incomingFileName);
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
                return errorMessageGenerator.CreateBadRequestResponce(InnerErrorCodes.Status400.InvalidScope);
            }

            if (fileName.IsNullOrWhiteSpace())
            {
                return errorMessageGenerator.CreateBadRequestResponce(InnerErrorCodes.Status400.InvalidFileName);
            }

            var uploadedImageData = await (from upi in apiDbContext!.Images! where upi.ScopeUid == scopeUid && upi.Name == fileName select upi).FirstOrDefaultAsync();

            if (uploadedImageData == null)
            {
                return errorMessageGenerator.CreateNotFoundResponce(InnerErrorCodes.Status404.FileNotFound, fileName);
            }

            return File(uploadedImageData!.Data, uploadedImageData.MimeType, fileName);
        }
    }
}