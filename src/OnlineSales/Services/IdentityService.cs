// <copyright file="IdentityService.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using OnlineSales.Entities;
using OnlineSales.Interfaces;

namespace OnlineSales.Services;

public class IdentityService : IIdentityService
{
    private readonly UserManager<User> userManager;

    public IdentityService(UserManager<User> userManager)
    {
        this.userManager = userManager;
    }

    public async Task<ClaimsPrincipal> CreateUserClaimsPrincipal(User user)
    {
        var claims = await CreateUserClaims(user);

        var identity = new ClaimsIdentity(claims);

        return new ClaimsPrincipal(identity);
    }

    public async Task<List<Claim>> CreateUserClaims(User user)
    {
        var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            };

        var roles = await userManager.GetRolesAsync(user);

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        return claims;
    }
}