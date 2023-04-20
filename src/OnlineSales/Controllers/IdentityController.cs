// <copyright file="IdentityController.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OnlineSales.Entities;
using Quartz.Impl.AdoJobStore.Common;

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

    [HttpGet("ExternalLogin")]
    public ActionResult ExternalLogin(string returnUrl)
    {
        const string provider = "OpenIdConnect";
        // var redirectUrl = Url.Action("ExternalLoginCallback");
        var properties = signInManager.ConfigureExternalAuthenticationProperties(provider, returnUrl);
        return new ChallengeResult(provider, properties);
    }

    [HttpGet("Callback")]
    public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
    {
        var info = await signInManager.GetExternalLoginInfoAsync();
        if (info == null)
        {
            throw new IdentityException("Failed to retrieve external login info");
        }

        var result = await signInManager.ExternalLoginSignInAsync(
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

        return LocalRedirect(returnUrl);
    }
}
