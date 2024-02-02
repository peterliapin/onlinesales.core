// <copyright file="HttpContextHelper.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using Microsoft.AspNetCore.Identity;
using Microsoft.Net.Http.Headers;
using OnlineSales.Entities;
using OnlineSales.Interfaces;

namespace OnlineSales.Infrastructure
{
    public class HttpContextHelper : IHttpContextHelper
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        public HttpContextHelper(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        public HttpRequest Request => httpContextAccessor?.HttpContext?.Request!;

        public string? IpAddress => httpContextAccessor?.HttpContext?.Connection?.RemoteIpAddress?.ToString();

        public string? UserAgent => httpContextAccessor?.HttpContext?.Request?.Headers[HeaderNames.UserAgent];

        public string? IpAddressV4 => httpContextAccessor?.HttpContext?.Connection?.RemoteIpAddress?.MapToIPv4().ToString();

        public string? IpAddressV6 => httpContextAccessor?.HttpContext?.Connection?.RemoteIpAddress?.MapToIPv6().ToString();

        public async Task<User?> GetCurrentUserAsync()
        {
            var userManager = httpContextAccessor?.HttpContext?.RequestServices.GetService<UserManager<User>>()!;

            return await UserHelper.GetCurrentUserAsync(userManager, httpContextAccessor?.HttpContext?.User);
        }

        public async Task<string?> GetCurrentUserIdAsync()
        {
            var userManager = httpContextAccessor?.HttpContext?.RequestServices.GetService<UserManager<User>>()!;

            return await UserHelper.GetCurrentUserIdAsync(userManager, httpContextAccessor?.HttpContext?.User);
        }
    }
}