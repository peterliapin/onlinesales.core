// <copyright file="AzureAdCookieEventsHandler.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using OnlineSales.Entities;
using OnlineSales.Infrastructure;

namespace OnlineSales.Identity;
public class AzureAdCookieEventsHandler : CookieAuthenticationEvents
{
    public override async Task SigningIn(CookieSigningInContext context)
    {
        var signInManager = context.HttpContext.RequestServices.GetService<SignInManager<User>>()!;
        var userManager = context.HttpContext.RequestServices.GetService<UserManager<User>>()!;

        var userEmail = context.Principal?.Claims.FirstOrDefault(claim => claim.Type.Contains("emailaddress"))?.Value ?? string.Empty;

        if (string.IsNullOrWhiteSpace(userEmail))
        {
            await this.RedirectToAccessDenied(((PropertiesContext<CookieAuthenticationOptions>)context as RedirectContext<CookieAuthenticationOptions>)!);
            return;
        }

        context.HttpContext.User = await IdentityHelper.TryLoginOnRegister(signInManager, userManager, userEmail, "AzureAdCookie");
    }
}
