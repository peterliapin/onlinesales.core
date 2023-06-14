// <copyright file="User.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using Microsoft.AspNetCore.Identity;
using OnlineSales.DataAnnotations;

namespace OnlineSales.Entities;
public class User : IdentityUser
{
    public string AvatarUrl { get; set; } = string.Empty;

    public DateTime? CreatedAt { get; set; }

    public DateTime? LastTimeLoggedIn { get; set; }

    [Searchable]
    public string DisplayName { get; set; } = string.Empty;
}