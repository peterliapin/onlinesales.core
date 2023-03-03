// <copyright file="AuthController.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineSales.Plugin.AzureAD.Entities;
using OnlineSales.Plugin.AzureAD.Exceptions;

namespace OnlineSales.Plugin.AzureAD.Controllers;

[Route("api/auth")]
public class AuthController : ControllerBase
{
    public AuthController()
    {
    }

    [HttpGet("login")]
    public async Task Login(string redirectUrl)
    {
        await HttpContext.ChallengeAsync("WebAppAuthorization", new AuthenticationProperties() { RedirectUri = redirectUrl });
    }

    [HttpGet("logout")]
    public async Task Logout(string redirectUrl)
    {
        await HttpContext.SignOutAsync("Cookies", new AuthenticationProperties() { RedirectUri = redirectUrl });
    }

    [HttpGet("Profile")]
    [Authorize]
    public ActionResult Profile()
    {
        if (HttpContext.User.Identity == null)
        {
            throw new MissingIdentityException();
        }

        return Ok(new User(HttpContext.User.Identity));
    }
}
