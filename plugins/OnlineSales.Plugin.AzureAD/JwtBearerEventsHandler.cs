// <copyright file="JwtBearerEventsHandler.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace OnlineSales.Plugin.AzureAD;
public class JwtBearerEventsHandler : JwtBearerEvents
{
    public override async Task TokenValidated(TokenValidatedContext context)
    {
        await context.HttpContext.SignInAsync("Cookies", context.Principal!);
        await base.TokenValidated(context);
    }
}