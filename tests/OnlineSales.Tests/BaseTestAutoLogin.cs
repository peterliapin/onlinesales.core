// <copyright file="BaseTestAutoLogin.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace OnlineSales.Tests;

public class BaseTestAutoLogin : BaseTest
{
    protected static readonly string LoginApi = "/api/identity/login";
    protected static readonly LoginDto AdminLoginData = new LoginDto()
    {
        Email = "admin@admin.com",
        Password = "adminPass!123",
    };

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
}
