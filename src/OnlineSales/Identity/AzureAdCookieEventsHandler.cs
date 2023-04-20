// <copyright file="AzureAdCookieEventsHandler.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using OnlineSales.Entities;

namespace OnlineSales.Identity;
public class AzureAdCookieEventsHandler : CookieAuthenticationEvents
{
    public override async Task SigningIn(CookieSigningInContext context)
    {
        var signInManager = context.HttpContext.RequestServices.GetService<SignInManager<User>>() !;
        var userManager = context.HttpContext.RequestServices.GetService<UserManager<User>>() !;

        var userEmail = context.Principal?.Claims.FirstOrDefault(claim => claim.Type.Contains("emailaddress"))?.Value ?? string.Empty;

        if (string.IsNullOrWhiteSpace(userEmail))
        {
            await RedirectToAccessDenied(((PropertiesContext<CookieAuthenticationOptions>)context as RedirectContext<CookieAuthenticationOptions>) !);
            return;
        }

        var user = await userManager.FindByEmailAsync(userEmail);
        if (user == null)
        {
            user = new User
            {
                UserName = userEmail,
                Email = userEmail,
                CreatedAt = DateTime.UtcNow,
                DisplayName = userEmail,
            };
            await userManager.CreateAsync(user);
        }

        user.LastTimeLoggedIn = DateTime.UtcNow;
        await userManager.UpdateAsync(user);

        await signInManager.SignInAsync(user, false, "AzureAdCookie");
        context.HttpContext.User = await signInManager.CreateUserPrincipalAsync(user);
    }
}
