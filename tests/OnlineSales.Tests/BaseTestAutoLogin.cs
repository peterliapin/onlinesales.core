// <copyright file="BaseTestAutoLogin.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;
using OnlineSales.Configuration;

namespace OnlineSales.Tests;

public class BaseTestAutoLogin : BaseTest
{
    protected static readonly string LoginApi = "/api/identity/login";
    protected static readonly LoginDto AdminLoginData = GetLoginDtoFromConfiguration();

    private JWTokenDto? authToken;

    public BaseTestAutoLogin()
        : base()
    {
        LoginAsAdmin().Wait();
    }

    protected async Task<JWTokenDto?> LoginAsAdmin()
    {
        authToken = await PostTest<JWTokenDto>(LoginApi, AdminLoginData, HttpStatusCode.OK);
        return authToken;
    }

    protected void Logout()
    {
        authToken = null;
    }

    protected override AuthenticationHeaderValue? GetAuthenticationHeaderValue()
    {
        return authToken != null ? new AuthenticationHeaderValue("Bearer", authToken!.Token) : null;
    }

    private static LoginDto GetLoginDtoFromConfiguration()
    {
        var defaultUser = Program.GetApp()!.Configuration.GetSection("DefaultUsers").Get<DefaultUsersConfig>()![0];
        return new LoginDto()
        {
            Email = defaultUser.Email,
            Password = defaultUser.Password,
        };
    }
}