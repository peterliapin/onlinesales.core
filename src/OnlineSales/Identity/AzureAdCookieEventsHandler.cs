// <copyright file="AzureAdCookieEventsHandler.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using OnlineSales.Entities;
using OnlineSales.Interfaces;

namespace OnlineSales.Identity;

public class AzureAdCookieEventsHandler : CookieAuthenticationEvents
{
    public override async Task SigningIn(CookieSigningInContext context)
    {
        var identityService = context.HttpContext.RequestServices.GetService<IIdentityService>()!;
        var signInManager = context.HttpContext.RequestServices.GetService<SignInManager<User>>()!;
        var userManager = context.HttpContext.RequestServices.GetService<UserManager<User>>()!;

        var email = context.Principal?.Claims.FirstOrDefault(claim => claim.Type.Contains("emailaddress"))?.Value ?? string.Empty;

        if (string.IsNullOrWhiteSpace(email))
        {
            await RedirectToAccessDenied(((PropertiesContext<CookieAuthenticationOptions>)context as RedirectContext<CookieAuthenticationOptions>)!);
            return;
        }

        var user = await identityService.FindOnRegister(email);

        user.LastTimeLoggedIn = DateTime.UtcNow;
        await userManager.UpdateAsync(user);

        var userPrincipal = await signInManager.CreateUserPrincipalAsync(user);

        await signInManager.SignInAsync(user, false, "AzureAdCookie");

        context.HttpContext.User = userPrincipal;
    }
}