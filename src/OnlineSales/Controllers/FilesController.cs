// <copyright file="FilesController.cs" company="WavePoint Co. Ltd.">
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
using OnlineSales.Helpers;

namespace OnlineSales.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class FilesController : ControllerBase
    {
        private readonly PgDbContext pgDbContext;

        public FilesController(PgDbContext pgDbContext)
        {
            this.pgDbContext = pgDbContext;
        }

        [HttpPost]
        [ProducesResponseType(typeof(FileDetailsDto), StatusCodes.Status201Created)]
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

            var scopeAndFileExists = await pgDbContext!.Files!.Where(e => e.ScopeUid == fileCreateDto.ScopeUid && e.Name == incomingFileName!).ToArrayAsync();
            if (scopeAndFileExists.Any())
            {
                var uploadedFile = scopeAndFileExists!.FirstOrDefault();
                uploadedFile!.Data = fileInBytes;
                uploadedFile!.Size = incomingFileSize;

                pgDbContext.Files!.Update(uploadedFile);
            }
            else
            {
                var uploadedFile = new Entities.File()
                {
                    Name = incomingFileName,
                    Size = incomingFileSize,
                    Data = fileInBytes,
                    MimeType = incomingFileMimeType!,
                    ScopeUid = fileCreateDto.ScopeUid.Trim(),
                    Extension = incomingFileExtension,
                };

                await pgDbContext.Files!.AddAsync(uploadedFile);
            }

            await pgDbContext.SaveChangesAsync();

            Log.Information("Request scheme {0}", HttpContext.Request.Scheme);
            Log.Information("Request host {0}", HttpContext.Request.Host.Value);

            var fileData = new FileDetailsDto()
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

            var scope = Path.GetDirectoryName(pathToFile)!.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            var fname = Path.GetFileName(pathToFile);

            var uploadedFileData = await pgDbContext!.Files!.Where(e => e.ScopeUid == scope && e.Name == fname).FirstOrDefaultAsync();

            return uploadedFileData == null
                ? throw new EntityNotFoundException(nameof(Entities.File), $"{pathToFile}")
                : (ActionResult)File(uploadedFileData!.Data, uploadedFileData.MimeType, uploadedFileData.Name);
        }
    }
}