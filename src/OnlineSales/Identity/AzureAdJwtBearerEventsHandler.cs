// <copyright file="AzureAdJwtBearerEventsHandler.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using OnlineSales.Entities;
using OnlineSales.Infrastructure;

namespace OnlineSales.Identity;
public class AzureAdJwtBearerEventsHandler : JwtBearerEvents
{
    public override async Task TokenValidated(TokenValidatedContext context)
    {
        var signInManager = context.HttpContext.RequestServices.GetService<SignInManager<User>>()!;
        var userManager = context.HttpContext.RequestServices.GetService<UserManager<User>>()!;

        var userEmail = context.Principal?.Claims.FirstOrDefault(claim => claim.Type.Contains("emailaddress"))?.Value ?? string.Empty;

        if (string.IsNullOrWhiteSpace(userEmail))
        {
            await AuthenticationFailed(new AuthenticationFailedContext(context.HttpContext, context.Scheme, context.Options));
            return;
        }

        var claimsPrimcipal = await IdentityHelper.TryLoginOnRegister(signInManager, userManager, userEmail, "AzureAdBearer");

        context.HttpContext.User = claimsPrimcipal;
    }
}