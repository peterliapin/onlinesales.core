// <copyright file="StorageController.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using OnlineSales.Data;
using OnlineSales.DTOs;
using OnlineSales.Entities;
using OnlineSales.Helpers;

namespace OnlineSales.Controllers
{
    // [Authorize]
    [Route("api/[controller]")]
    public class StorageController : ControllerBase
    {
        private readonly PgDbContext pgDbContext;

        public StorageController(PgDbContext pgDbContext)
        {
            this.pgDbContext = pgDbContext;
        }

        [HttpPost]
        [AllowAnonymous]
        [ProducesResponseType(typeof(MediaDetailsDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> Post([FromForm] FileCreateDto fileCreateDto)
        {
            var provider = new FileExtensionContentTypeProvider();

            fileCreateDto.ScopeUid = fileCreateDto.ScopeUid.TrimStart('/');

            var incomingFileName = fileCreateDto.File!.FileName.ToTranslit().Slugify()!;
            var incomingFileExtension = Path.GetExtension(fileCreateDto.File!.FileName);
            var incomingFileSize = fileCreateDto.File!.Length; // bytes
            var incomingFileMimeType = string.Empty;

            if (!provider.TryGetContentType(incomingFileName, out incomingFileMimeType))
            {
                ModelState.AddModelError("FileName", "Unsupported MIME type");

                throw new InvalidModelStateException(ModelState);
            }

            using var fileStream = fileCreateDto.File.OpenReadStream();
            var fileInBytes = new byte[incomingFileSize];
            fileStream.Read(fileInBytes, 0, (int)fileCreateDto.File.Length);

            var scopeAndFileExists = await pgDbContext!.StoredFiles!.Where(e => e.ScopeUid == fileCreateDto.ScopeUid && e.Name == incomingFileName!).ToArrayAsync();
            if (!scopeAndFileExists.Any())
            {
                StoredFile uploadedMedia = new()
                {
                    Name = incomingFileName,
                    Size = incomingFileSize,
                    Data = fileInBytes,
                    MimeType = incomingFileMimeType!,
                    ScopeUid = fileCreateDto.ScopeUid.Trim(),
                    Extension = incomingFileExtension,
                };

                await pgDbContext.StoredFiles!.AddAsync(uploadedMedia);
            }

            await pgDbContext.SaveChangesAsync();

            Log.Information("Request scheme {0}", HttpContext.Request.Scheme);
            Log.Information("Request host {0}", HttpContext.Request.Host.Value);

            var fileData = new MediaDetailsDto()
            {
                Location = Path.Combine(HttpContext.Request.Path, fileCreateDto.ScopeUid, incomingFileName).Replace("\\", "/"),
            };
            return CreatedAtAction(nameof(Get), new { scopeUid = fileCreateDto.ScopeUid, fileName = incomingFileName }, fileData);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("{*pathToFile}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> Get([Required] string pathToFile)
        {
            pathToFile = Uri.UnescapeDataString(pathToFile);
            var apiRequestPath = "/api/storage/";

            if (pathToFile.StartsWith(apiRequestPath))
            {
                pathToFile = pathToFile[(apiRequestPath.Length!)..];
            }

            var scope = Path.GetDirectoryName(pathToFile)!.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            var fname = Path.GetFileName(pathToFile);

            var uploadedImageData = await pgDbContext!.StoredFiles!.Where(e => e.ScopeUid == scope && e.Name == fname).FirstOrDefaultAsync();

            return uploadedImageData == null
                ? throw new EntityNotFoundException(nameof(Media), $"{pathToFile}")
                : (ActionResult)File(uploadedImageData!.Data, uploadedImageData.MimeType, uploadedImageData.Name);
        }
    }
}