// <copyright file="IdentityTests.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OnlineSales.Configuration;

namespace OnlineSales.Tests;

public class IdentityLoginTests : BaseTestAutoLogin
{
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
        token!.Token.Should().NotBeEmpty();
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
        for (int i = 0; i < count; i++)
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

    private async Task TestBody(string username, string password, HttpStatusCode expectedCode)
    {
        var testLoginDto = new LoginDto()
        { Email = username, Password = password };
        Assert.NotEqual(testLoginDto, AdminLoginData);

        var token = await PostTest<JWTokenDto>(LoginApi, testLoginDto, expectedCode);
        token.Should().BeNull();
    }
}