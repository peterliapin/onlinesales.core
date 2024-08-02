// <copyright file="AzureAdJwtBearerEventsHandler.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using OnlineSales.Entities;
using OnlineSales.Interfaces;

namespace OnlineSales.Identity;

public class AzureAdJwtBearerEventsHandler : JwtBearerEvents
{
    private readonly ConfigurationManager configurationManager;

    public AzureAdJwtBearerEventsHandler(ConfigurationManager configurationManager)
    {
        this.configurationManager = configurationManager;
    }

    public override async Task TokenValidated(TokenValidatedContext context)
    {
        var identityService = context.HttpContext.RequestServices.GetService<IIdentityService>()!;
        var userManager = context.HttpContext.RequestServices.GetService<UserManager<User>>()!;

        var email = context.Principal?.Claims.FirstOrDefault(claim => claim.Type.Contains("emailaddress"))?.Value ?? string.Empty;

        if (string.IsNullOrWhiteSpace(email))
        {
            await AuthenticationFailed(new AuthenticationFailedContext(context.HttpContext, context.Scheme, context.Options));
            return;
        }

        var claimsIdentity = context.Principal?.Identity as ClaimsIdentity;

        var administratorsGroupId = configurationManager.GetValue<string>("AzureAd:GroupsMapping:Administrators");

        var adminRole = claimsIdentity!.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.Role && c.Value == administratorsGroupId);

        if (adminRole != null)
        {
            claimsIdentity!.AddClaim(new Claim(ClaimTypes.Role, "Admin"));
        }

        var user = await identityService.FindOnRegister(email);

        user.LastTimeLoggedIn = DateTime.UtcNow;
        await userManager.UpdateAsync(user);

        var claims = await identityService.CreateUserClaims(user);
        claimsIdentity.AddClaims(claims);
    }
}