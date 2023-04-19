// <copyright file="JwtBearerEventsHandler.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using OnlineSales.Entities;

namespace OnlineSales.Plugin.AzureAD;

public class JwtBearerEventsHandler : JwtBearerEvents
{
    public override async Task TokenValidated(TokenValidatedContext context)
    {
        var signInManager = context.HttpContext.RequestServices.GetService<SignInManager<User>>() !;
        var userManager = context.HttpContext.RequestServices.GetService<UserManager<User>>() !;

        var userEmail = context.Principal!.Identity!.Name;

        if (string.IsNullOrWhiteSpace(userEmail))
        {
            await AuthenticationFailed(new AuthenticationFailedContext(context.HttpContext, context.Scheme, context.Options));
            return;
        }

        var user = await userManager.FindByEmailAsync(userEmail);
        if (user == null)
        {
            user = new User
            {
                UserName = userEmail,
                Email = userEmail,
            };
            await userManager.CreateAsync(user);
        }

        await signInManager.SignInWithClaimsAsync(user, null, context.Principal.Claims);
        await context.HttpContext.SignInAsync("Cookies", context.Principal!);
        await base.TokenValidated(context);
    }
}