// <copyright file="IdentityController.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication;
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
    private readonly IOptions<AzureADConfig> azureAdConfig;

    public IdentityController(
        SignInManager<User> signInManager,
        IOptions<JwtConfig> jwtConfig,
        IOptions<AzureADConfig> azureAdConfig)
    {
        this.signInManager = signInManager;
        this.jwtConfig = jwtConfig;
        this.azureAdConfig = azureAdConfig;
    }

    [HttpGet("azure-login")]
    public IActionResult AzureLogin(string returnUrl = "/")
    {
        // Check if Azure AD is properly configured
        if (string.IsNullOrEmpty(azureAdConfig.Value.TenantId) ||
            azureAdConfig.Value.TenantId == "$AZUREAD__TENANTID")
        {
            return BadRequest("Azure AD authentication is not configured.");
        }

        // Use the AzureAd OpenID Connect scheme for the challenge
        var redirectUri = Url.Action(nameof(AzureLoginCallback), new { returnUrl });
        var properties = new AuthenticationProperties { RedirectUri = redirectUri };

        // Challenge with Azure AD OpenID Connect scheme
        return Challenge(properties, "AzureAdOpenID");
    }

    [HttpGet("azure-login-callback")]
    public async Task<IActionResult> AzureLoginCallback(string returnUrl = "/")
    {
        // At this point, the user should be authenticated by Azure AD
        if (!User.Identity?.IsAuthenticated ?? true)
        {
            return Unauthorized("Azure AD authentication failed.");
        }

        // Get the Azure AD access token if available
        var accessToken = await HttpContext.GetTokenAsync("AzureAdOpenID", "access_token");

        // Redirect back to the client application with the token
        // In a real implementation, you might need a more secure way to transmit the token
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect($"{returnUrl}?token={accessToken}");
        }

        // Token response for API clients
        return Ok(new { token = accessToken, tokenType = "Bearer" });
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

        if (!user.EmailConfirmed)
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

        // Update last login time
        user.LastTimeLoggedIn = DateTime.UtcNow;
        await userManager.UpdateAsync(user);

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