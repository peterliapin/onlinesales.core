// <copyright file="IdentityController.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OnlineSales.Entities;

namespace OnlineSales.Controllers;

[AllowAnonymous]
[Route("api/[controller]")]
public class IdentityController : ControllerBase
{
    private readonly SignInManager<User> signInManager;

    public IdentityController(SignInManager<User> signInManager)
    {
        this.signInManager = signInManager;
    }

    [HttpGet("external-login")]
    public ActionResult ExternalLogin(string returnUrl)
    {
        const string provider = "OpenIdConnect";
        // var redirectUrl = Url.Action("ExternalLoginCallback");
        var properties = this.signInManager.ConfigureExternalAuthenticationProperties(provider, returnUrl);
        return new ChallengeResult(provider, properties);
    }

    [HttpGet("callback")]
    public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
    {
        var info = await this.signInManager.GetExternalLoginInfoAsync();
        if (info == null)
        {
            throw new IdentityException("Failed to retrieve external login info");
        }

        var result = await this.signInManager.ExternalLoginSignInAsync(
            info.LoginProvider,
            info.ProviderKey,
            isPersistent: false,
            bypassTwoFactor: true);
        if (!result.Succeeded)
        {
            if (result.IsLockedOut)
            {
                throw new IdentityException("Account locked out");
            }

            if (result.IsNotAllowed)
            {
                throw new IdentityException("Sign in with specified account is prohibited");
            }
        }

        return this.LocalRedirect(returnUrl);
    }
}
