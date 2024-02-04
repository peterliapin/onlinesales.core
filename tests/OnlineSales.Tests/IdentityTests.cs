// <copyright file="IdentityTests.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace OnlineSales.Tests;

public class IdentityLoginTests : BaseTest
{
    private static readonly string Api = "/api/identity/login";
    private static readonly LoginDto CorrectLoginData = new LoginDto()
    {
        Email = "admin@admin.com",
        Password = "adminPass!123",
    };

    [Theory]
    [InlineData("UnexpectedUser@admin.com", "WrongPassword")]
    [InlineData("UnexpectedUser@admin.com", "adminPass!123")]
    [InlineData("UnexpectedUser@admin.com", "")]
    public async Task LoginNotFoundTest(string username, string password)
    {
        var contentLoginDto = new LoginDto()
        { Email = username, Password = password };
        Assert.NotEqual(contentLoginDto, CorrectLoginData);

        var token = await PostTest<JWTokenDto>(Api, contentLoginDto, HttpStatusCode.NotFound, string.Empty);
        token.Should().BeNull();
    }

    [Theory]
    [InlineData("admin@admin.com", "WrongPassword")]
    [InlineData("admin@admin.com", "")]
    public async Task LoginUnauthorizedTest(string username, string password)
    {
        var contentLoginDto = new LoginDto()
        { Email = username, Password = password };
        Assert.NotEqual(contentLoginDto, CorrectLoginData);

        var token = await PostTest<JWTokenDto>(Api, contentLoginDto, HttpStatusCode.Unauthorized, string.Empty);
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
        Assert.NotEqual(contentLoginDto, CorrectLoginData);

        var token = await PostTest<JWTokenDto>(Api, contentLoginDto, HttpStatusCode.Forbidden, string.Empty);
        token.Should().BeNull();
    }

    [Fact]
    public async Task LoginOkTest()
    {
        var token = await PostTest<JWTokenDto>(Api, CorrectLoginData, HttpStatusCode.OK, string.Empty);
        token.Should().NotBeNull();
        token!.Token.Should().NotBeEmpty();
    }
}
