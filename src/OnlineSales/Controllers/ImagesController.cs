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
        public async Task<ActionResult> Post([FromForm] ImageDtos imageDtos)
        {
            var provider = new FileExtensionContentTypeProvider();

            string incomingFileName = imageDtos.Image!.FileName;
            string incomingFileExtension = Path.GetExtension(imageDtos.Image!.FileName);
            long incomingFileSize = imageDtos.Image!.Length; // bytes
            string? incomingFileMimeType = string.Empty;
            string returnedFileName = RandomString(5) + DateTime.Now.Year.ToString() + new Random().Next(1000, 9999).ToString() + "-" + DateTime.Now.Month.ToString("00") + DateTime.Now.Day.ToString("00") + RandomString(5);

            provider.TryGetContentType(incomingFileName, out incomingFileMimeType);

            using var fileStream = imageDtos.Image.OpenReadStream();
            byte[] imageInBytes = new byte[incomingFileSize];
            fileStream.Read(imageInBytes, 0, (int)imageDtos.Image.Length);

            string baseUrl = $"{Request.Scheme}://{Request.Host.Value}{Request.Path}";

            UploadedImage uploadedImage = new ()
            {
                FileName = incomingFileName,
                FileSize = incomingFileSize,
                ImageBinaryData = imageInBytes,
                MimeType = incomingFileMimeType!,
                ScopeId = imageDtos.ScopeId,
                ReturnedFileName = returnedFileName,
                FileExtension = incomingFileExtension,
                CreatedAt = DateTime.UtcNow,
                CreatedByIP = GetClientIP(),
                CreatedByUserAgent = GetUserAgent(),
            };

            await apiDbContext.UploadedImages!.AddAsync(uploadedImage);

            await apiDbContext.SaveChangesAsync();

            return Ok(new { ImageUrl = baseUrl + "/" + imageDtos.ScopeId + "/" + returnedFileName + incomingFileExtension });
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

            var uploadedImageData = await (from upi in apiDbContext!.UploadedImages! where upi.ScopeId == scopeId && upi.ReturnedFileName + upi.FileExtension == fileName select upi).FirstOrDefaultAsync();

            if (uploadedImageData == null)
            {
                return BadRequest("Requested file not found");
            }

            return File(uploadedImageData!.ImageBinaryData, uploadedImageData.MimeType, fileName); 
        }

        private string? GetClientIP()
        {
            return HttpContext?.Connection?.RemoteIpAddress?.ToString();
        }

        private string? GetUserAgent()
        {
            return HttpContext?.Request?.Headers[HeaderNames.UserAgent];
        }

        private string RandomString(int length)
        {
            Random rand = new Random();
            string charbase = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Range(0, length)
                   .Select(_ => charbase[rand.Next(charbase.Length)])
                   .ToArray());
        }
    }
}