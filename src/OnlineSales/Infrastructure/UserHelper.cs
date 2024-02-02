// <copyright file="UserHelper.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using OnlineSales.Entities;

namespace OnlineSales.Infrastructure
{
    public static class UserHelper
    {
        public static async Task<string?> GetCurrentUserIdAsync(UserManager<User> userManager, ClaimsPrincipal? claimsPrincipal)
        {
            if (claimsPrincipal == null)
            {
                return null;
            }

            var userId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                return userId;
            }

            var user = await GetCurrentUserAsync(userManager, claimsPrincipal);

            if (user != null)
            {
                return user.Id;
            }

            return null;
        }

        public static async Task<User?> GetCurrentUserAsync(UserManager<User> userManager, ClaimsPrincipal? claimsPrincipal)
        {
            if (claimsPrincipal == null || claimsPrincipal.Identity == null)
            {
                return null;
            }

            if (claimsPrincipal.Identity.IsAuthenticated && !claimsPrincipal.Identity!.AuthenticationType!.Contains("Federation"))
            {
                return await userManager.FindByNameAsync(claimsPrincipal.Identity.Name!);
            }

            var userEmail = claimsPrincipal.Claims.FirstOrDefault(claim => claim.Type.Contains("emailaddress"))?.Value ?? string.Empty;

            if (string.IsNullOrWhiteSpace(userEmail))
            {
                return null;                
            }

            return await userManager.FindByEmailAsync(userEmail);
        }
    }
}