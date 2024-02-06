// <copyright file="IdentityTests.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OnlineSales.Configuration;

namespace OnlineSales.Tests;

public class IdentityTests : BaseTestAutoLogin
{
    private static readonly string RefreshApi = "/api/identity/refresh";

    [Theory]
    [InlineData("admin@admin.com", "")]
    [InlineData("UnexpectedUser@admin.com", "")]
    [InlineData("wrong address", "AnyPassword")]
    public async Task LoginBadParamsTest(string username, string password)
    {
        await TestBody(username, password, HttpStatusCode.UnprocessableEntity);
    }

    [Theory]
    [InlineData("admin@admin.com", "WrongPassword")]
    [InlineData("UnexpectedUser@admin.com", "WrongPassword")]
    [InlineData("UnexpectedUser@admin.com", "adminPass!123")]
    public async Task LoginUnauthorizedTest(string username, string password)
    {
        await TestBody(username, password, HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData("admin@admin.com", "WrongPassword")]
    [InlineData("admin@admin.com", "adminPass!123")]
    public async Task LoginNotConfirmedEmailTest(string username, string password)
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

        await TestBody(username, password, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task LoginOkTest()
    {
        var token = await PostTest<JWTokenDto>(LoginApi, AdminLoginData, HttpStatusCode.OK);
        token.Should().NotBeNull();
        token!.AccessToken.Should().NotBeEmpty();
    }

    [Fact]
    public async Task LoginLogoutTest()
    {
        string testApi = "/api/links";

        GetAuthenticationHeaderValue().Should().NotBeNull();
        await GetTest(testApi, HttpStatusCode.OK);

        Logout();
        GetAuthenticationHeaderValue().Should().BeNull();
        await GetTest(testApi, HttpStatusCode.Unauthorized);

        await LoginAsAdmin();
        GetAuthenticationHeaderValue().Should().NotBeNull();
        await GetTest(testApi, HttpStatusCode.OK);

        Logout();
        GetAuthenticationHeaderValue().Should().BeNull();
        await GetTest(testApi, HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task LockoutTest()
    {
        var config = Program.GetApp()!.Configuration;
        var lockoutConfig = config.GetSection("Identity").Get<IdentityConfig>();
        lockoutConfig.Should().NotBeNull();

        var testLoginDto = new LoginDto()
        { Email = AdminLoginData.Email, Password = "WrongPassword" };
        // The first times login returns Unauthorized
        int count = lockoutConfig!.MaxFailedAccessAttempts - 1;
        for(int i = 0; i < count; i++)
        {
            await PostTest<JWTokenDto>(LoginApi, testLoginDto, HttpStatusCode.Unauthorized);
        }

        // When maximum number of failed attemts is achived login returns TooManyRequests and blocks the user
        await PostTest<JWTokenDto>(LoginApi, testLoginDto, HttpStatusCode.TooManyRequests);
        // When the user is blocked login returns BadRequest
        await PostTest<JWTokenDto>(LoginApi, testLoginDto, HttpStatusCode.BadRequest);
        await PostTest<JWTokenDto>(LoginApi, testLoginDto, HttpStatusCode.BadRequest);
        // White untin the user is unlocked automatically
        await Task.Delay(TimeSpan.FromMinutes(lockoutConfig!.LockoutTime * 1.1));
        await PostTest<JWTokenDto>(LoginApi, testLoginDto, HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RefreshBadParamTest()
    {
        string emptyToken = string.Empty;
        var refreshDto = new RefreshTokenDto()
        { RefreshToken = emptyToken };
        await PostTest<JWTokenDto>(RefreshApi, refreshDto, HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task RefreshNotLoggedInTest()
    {
        var refreshDto = new RefreshTokenDto()
        { RefreshToken = Guid.NewGuid().ToString() };
        await PostTest<JWTokenDto>(RefreshApi, refreshDto, HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RefreshLockedUserTest()
    {
        // Get token until the user blocked
        var token = await GetTokens(AdminLoginData);
        // Block the user
        using (var scope = App.Services.CreateScope())
        {
            var config = Program.GetApp()!.Configuration;
            var lockoutConfig = config.GetSection("Identity").Get<IdentityConfig>();

            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            Assert.NotNull(userManager);
            var user = await userManager.FindByEmailAsync(AdminLoginData.Email);
            Assert.NotNull(user);

            for(int i = 0; i < lockoutConfig!.MaxFailedAccessAttempts + 1; i++)
            {
                await userManager.AccessFailedAsync(user);
            }

            // Admin must be blocked here
            Assert.True(await userManager.IsLockedOutAsync(user));
        }

        var refreshDto = new RefreshTokenDto()
        { RefreshToken = token!.RefreshToken };
        await PostTest<JWTokenDto>(RefreshApi, refreshDto, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RefreshTokenExpiredTest()
    {
        var token = await GetTokens(AdminLoginData);
        using (var scope = App.Services.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            Assert.NotNull(userManager);
            var user = await userManager.FindByEmailAsync(AdminLoginData.Email);
            Assert.NotNull(user);
            Assert.NotNull(user.RefreshToken);
            user.RefreshTokenValidTo = DateTime.UtcNow - TimeSpan.FromHours(10);
            await userManager.UpdateAsync(user);
        }

        var refreshDto = new RefreshTokenDto()
        { RefreshToken = token!.RefreshToken };
        await PostTest<JWTokenDto>(RefreshApi, refreshDto, HttpStatusCode.Unauthorized);

        using (var scope = App.Services.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            Assert.NotNull(userManager);
            var user = await userManager.FindByEmailAsync(AdminLoginData.Email);
            Assert.NotNull(user);
            Assert.Null(user.RefreshToken);
            Assert.Null(user.RefreshTokenValidTo);
        }
    }

    [Fact]
    public async Task RefreshAccessTokenTest()
    {
        var token = await GetTokens(AdminLoginData);

        var refreshDto = new RefreshTokenDto()
        { RefreshToken = token!.RefreshToken };
        var token2 = await PostTest<JWTokenDto>(RefreshApi, refreshDto, HttpStatusCode.OK);

        token2.Should().NotBeNull();
        Assert.Equal(token.RefreshToken, token2!.RefreshToken);
        Assert.NotEqual(token.AccessToken, token2!.AccessToken);
    }

    private async Task TestBody(string username, string password, HttpStatusCode expectedCode)
    {
        var testLoginDto = new LoginDto()
        { Email = username, Password = password };
        Assert.NotEqual(testLoginDto, AdminLoginData);

        var token = await PostTest<JWTokenDto>(LoginApi, testLoginDto, expectedCode);
        token.Should().BeNull();
    }

    private async Task<JWTokenDto?> GetTokens(LoginDto input)
    {
        return await PostTest<JWTokenDto>(LoginApi, input, HttpStatusCode.OK);
    }
}