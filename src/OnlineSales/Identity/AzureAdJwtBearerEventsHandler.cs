// <copyright file="AzureAdJwtBearerEventsHandler.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using OnlineSales.Entities;
using OnlineSales.Interfaces;

namespace OnlineSales.Identity;

public class AzureAdJwtBearerEventsHandler : JwtBearerEvents
{
    public override async Task TokenValidated(TokenValidatedContext context)
    {
        var identityService = context.HttpContext.RequestServices.GetService<IIdentityService>()!;
        var signInManager = context.HttpContext.RequestServices.GetService<SignInManager<User>>()!;
        var userManager = context.HttpContext.RequestServices.GetService<UserManager<User>>()!;

        var email = context.Principal?.Claims.FirstOrDefault(claim => claim.Type.Contains("emailaddress"))?.Value ?? string.Empty;

        if (string.IsNullOrWhiteSpace(email))
        {
            await AuthenticationFailed(new AuthenticationFailedContext(context.HttpContext, context.Scheme, context.Options));
            return;
        }

        var user = await identityService.FindOnRegister(email);

        user.LastTimeLoggedIn = DateTime.UtcNow;
        await userManager.UpdateAsync(user);        

        var userPrincipal = await signInManager.CreateUserPrincipalAsync(user);
        
        await signInManager.SignInAsync(user, false, "AzureAdBearer");

        context.HttpContext.User = userPrincipal;
    }
}