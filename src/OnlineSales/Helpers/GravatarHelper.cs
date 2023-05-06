// <copyright file="GravatarHelper.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Security.Cryptography;
using System.Text;

namespace OnlineSales.Helpers;

public class GravatarHelper
{
    public static string EmailToGravatarUrl(string email)
    {
        var emailBytes = Encoding.ASCII.GetBytes(email);
        var emailHashCode = MD5.Create().ComputeHash(emailBytes);

        return "https://www.gravatar.com/avatar/" + Convert.ToHexString(emailHashCode).ToLower() + "?size=48&d=mp";
    }
}