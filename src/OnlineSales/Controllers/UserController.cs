// <copyright file="UserController.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Nest;
using OnlineSales.Configuration;
using OnlineSales.Data;
using OnlineSales.DTOs;
using OnlineSales.Entities;
using OnlineSales.Infrastructure;

namespace OnlineSales.Controllers;

[Authorize(AuthenticationSchemes = "Bearer")]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    protected readonly PgDbContext dbContext;
    protected readonly IMapper mapper;
    protected readonly UserManager<User> userManager;
    protected readonly SignInManager<User> signInManager;

    public UserController(PgDbContext dbContext, IMapper mapper, UserManager<User> userManager, SignInManager<User> signInManager)
    {
        this.dbContext = dbContext;
        this.mapper = mapper;
        this.userManager = userManager;
        this.signInManager = signInManager;
    }

    // GET api/profile
    [HttpGet("profile")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<User>> GetProfile()
    {
        var user = await UserHelper.GetCurrentUserAsync(userManager, this.User);
        return Ok(user);
    }

    // GET api/profile/5
    [HttpGet("profile/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> GetProfile(int id)
    {
        var users = await userManager.Users.Select(r => r.AvatarUrl).ToListAsync();
        return Ok(users);
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

        var scopeAndFileExists = from i in dbContext!.Media!
                                 where i.ScopeUid == imageCreateDto.ScopeUid.Trim() && i.Name == incomingFileName
                                 select i;
        if (scopeAndFileExists.Any())
        {
            var uploadedImage = scopeAndFileExists!.FirstOrDefault();
            uploadedImage!.Data = imageInBytes;
            uploadedImage!.Size = incomingFileSize;

            dbContext.Media!.Update(uploadedImage);
        }
        else
        {
            Media uploadedMedia = new ()
            {
                Name = incomingFileName,
                Size = incomingFileSize,
                Data = imageInBytes,
                MimeType = incomingFileMimeType!,
                ScopeUid = imageCreateDto.ScopeUid.Trim(),
                Extension = incomingFileExtension,
            };

            await dbContext.Media!.AddAsync(uploadedMedia);
        }

        await dbContext.SaveChangesAsync();

        var fileData = new MediaDetailsDto()
        {
            Location = Path.Combine(this.HttpContext.Request.Path, imageCreateDto.ScopeUid, incomingFileName).Replace("\\", "/"),
        };
        var filePath = CreatedAtAction(nameof(MediaController.Get), new { scopeUid = imageCreateDto.ScopeUid, fileName = incomingFileName }, fileData).UrlHelper!.Content!;
        return Ok(filePath);
    }
}
