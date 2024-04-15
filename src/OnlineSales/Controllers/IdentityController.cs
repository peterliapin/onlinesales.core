// <copyright file="IdentityController.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OnlineSales.Configuration;
using OnlineSales.DTOs;
using OnlineSales.Entities;

namespace OnlineSales.Controllers;

[AllowAnonymous]
[Route("api/[controller]")]
public class IdentityController : ControllerBase
{
    private readonly SignInManager<User> signInManager;
    private readonly IOptions<JwtConfig> jwtConfig;

    public IdentityController(SignInManager<User> signInManager, IOptions<JwtConfig> jwtConfig)
    {
        this.signInManager = signInManager;
        this.jwtConfig = jwtConfig;
    }

    [HttpGet("external-login")]
    public ActionResult ExternalLogin(string returnUrl)
    {
        const string provider = "OpenIdConnect";
        var properties = signInManager.ConfigureExternalAuthenticationProperties(provider, returnUrl);
        return new ChallengeResult(provider, properties);
    }

    [HttpGet("callback")]
    public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
    {
        var info = await signInManager.GetExternalLoginInfoAsync();
        if (info == null)
        {
            throw new IdentityException("Failed to retrieve external login info");
        }

        var result = await signInManager.ExternalLoginSignInAsync(
            info.LoginProvider,
            info.ProviderKey,
            isPersistent: false,
            bypassTwoFactor: true);

        if (!result.Succeeded)
        {
            if (result.IsLockedOut)
            {
                throw new IdentityException("Account locked out");
            }

            if (result.IsNotAllowed)
            {
                throw new IdentityException("Sign in with specified account is prohibited");
            }
        }

        return LocalRedirect(returnUrl);
    }

    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult> Login([FromBody] LoginDto input)
    {
        var userManager = signInManager.UserManager;

        var user = await userManager.FindByEmailAsync(input.Email);

        if (user == null)
        {
            throw new UnauthorizedException();
        }

        if(!user.EmailConfirmed)
        {
            throw new IdentityException("Email is not confirmed");
        }

        if (await userManager.IsLockedOutAsync(user))
        {
            throw new IdentityException("Account locked out");
        }

        var signResult = await signInManager.CheckPasswordSignInAsync(user, input.Password, true);
        
        if (!signResult.Succeeded)
        {
            if (signResult.IsLockedOut)
            {
                throw new TooManyRequestsException();
            }
            else
            {
                throw new UnauthorizedException();
            }
        }

        var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            };

        var roles = await userManager.GetRolesAsync(user);
        foreach (var role in roles)
        {
            authClaims.Add(new Claim(ClaimTypes.Role, role));
        }

        var token = GetToken(authClaims);

        return Ok(new JWTokenDto()
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            Expiration = token.ValidTo,
        });
    }

    private JwtSecurityToken GetToken(List<Claim> authClaims)
    {
        var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig.Value.Secret));

        var token = new JwtSecurityToken(
            issuer: jwtConfig.Value.Issuer,
            audience: jwtConfig.Value.Audience,
            expires: DateTime.Now.AddYears(1),
            claims: authClaims,
            signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256));

        return token;
    }
}