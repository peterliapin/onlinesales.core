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
        public static async Task<User?> GetCurrentUserAsync(UserManager<User> userManager, ClaimsPrincipal user)
        {
            if (user.Identity == null)
            {
                return null;
            }

            if (user.Identity.IsAuthenticated && !user.Identity!.AuthenticationType!.Contains("Federation"))
            {
                return await userManager.FindByNameAsync(user.Identity.Name!);
            }

            var userEmail = user.Claims.FirstOrDefault(claim => claim.Type.Contains("emailaddress"))?.Value ?? string.Empty;

            if (string.IsNullOrWhiteSpace(userEmail))
            {
                throw new UnauthorizedAccessException();
            }

            return await userManager.FindByEmailAsync(userEmail);
        }
    }
}