// <copyright file="AzureAdCookieEventsHandler.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using OnlineSales.Entities;
using OnlineSales.Infrastructure;
using OnlineSales.Interfaces;

namespace OnlineSales.Identity;

public class AzureAdCookieEventsHandler : CookieAuthenticationEvents
{
    public override async Task SigningIn(CookieSigningInContext context)
    {
        var signInManager = context.HttpContext.RequestServices.GetService<SignInManager<User>>()!;
        var userManager = context.HttpContext.RequestServices.GetService<UserManager<User>>()!;
        var identityService = context.HttpContext.RequestServices.GetService<IIdentityService>()!;

        var userEmail = context.Principal?.Claims.FirstOrDefault(claim => claim.Type.Contains("emailaddress"))?.Value ?? string.Empty;

        if (string.IsNullOrWhiteSpace(userEmail))
        {
            await RedirectToAccessDenied(((PropertiesContext<CookieAuthenticationOptions>)context as RedirectContext<CookieAuthenticationOptions>)!);
            return;
        }

        var claimsPrimcipal = await IdentityHelper.TryLoginOnRegister(signInManager, userManager, identityService, userEmail, "AzureAdCookie");

        context.HttpContext.User = claimsPrimcipal;
    }
}