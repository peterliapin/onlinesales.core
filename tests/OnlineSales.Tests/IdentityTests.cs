// <copyright file="IdentityTests.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace OnlineSales.Tests;

public class IdentityLoginTests : BaseTestAutoLogin
{
    [Theory]
    [InlineData("UnexpectedUser@admin.com", "WrongPassword")]
    [InlineData("UnexpectedUser@admin.com", "adminPass!123")]
    [InlineData("UnexpectedUser@admin.com", "")]
    public async Task LoginNotFoundTest(string username, string password)
    {
        var contentLoginDto = new LoginDto()
        { Email = username, Password = password };
        Assert.NotEqual(contentLoginDto, AdminLoginData);

        var token = await PostTest<JWTokenDto>(LoginApi, contentLoginDto, HttpStatusCode.NotFound);
        token.Should().BeNull();
    }

    [Theory]
    [InlineData("admin@admin.com", "WrongPassword")]
    [InlineData("admin@admin.com", "")]
    public async Task LoginUnauthorizedTest(string username, string password)
    {
        var contentLoginDto = new LoginDto()
        { Email = username, Password = password };
        Assert.NotEqual(contentLoginDto, AdminLoginData);

        var token = await PostTest<JWTokenDto>(LoginApi, contentLoginDto, HttpStatusCode.Unauthorized);
        token.Should().BeNull();
    }

    [Theory]
    [InlineData("admin@admin.com", "WrongPassword")]
    [InlineData("admin@admin.com", "")]
    [InlineData("admin@admin.com", "adminPass!123")]
    public async Task LoginForbiddenTest(string username, string password)
    {
        using (var scope = App.Services.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            Assert.NotNull(userManager);
            var user = await userManager.FindByEmailAsync(username);
            Assert.NotNull(user);
            user.EmailConfirmed = false;
            await userManager.UpdateAsync(user);
        }

        var contentLoginDto = new LoginDto()
        { Email = username, Password = password };
        Assert.NotEqual(contentLoginDto, AdminLoginData);

        var token = await PostTest<JWTokenDto>(LoginApi, contentLoginDto, HttpStatusCode.Forbidden);
        token.Should().BeNull();
    }

    [Fact]
    public async Task LoginOkTest()
    {
        var token = await PostTest<JWTokenDto>(LoginApi, AdminLoginData, HttpStatusCode.OK);
        token.Should().NotBeNull();
        token!.Token.Should().NotBeEmpty();
    }

    [Fact]
    public async Task LoginLogoutTest()
    {
        string testApi = "/api/links";

        GetAuthenticationHeaderValue().Should().NotBeNull();
        await GetTest(testApi, HttpStatusCode.OK);

        Logout();
        await GetTest(testApi, HttpStatusCode.Unauthorized);

        var token = await LoginAsAdmin();
        token.Should().NotBeNull();
        await GetTest(testApi, HttpStatusCode.OK);

        Logout();
        await GetTest(testApi, HttpStatusCode.Unauthorized);
    }
}
