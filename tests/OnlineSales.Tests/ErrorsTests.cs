// <copyright file="ErrorsTests.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using OnlineSales.DTOs;
using OnlineSales.Helpers;

namespace OnlineSales.Tests;

public class ErrorsTests : BaseTest
{
    [Fact]
    public async Task Http401ErrorTest()
    {
        var post = new TestPost();

        var response = await Request(HttpMethod.Post, "/api/posts", post, "Fail");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var responseContent = await response.Content.ReadAsStringAsync();

        responseContent.Should().Be(string.Empty);
    }

    [Fact]
    public async Task Http422ErrorTest()
    {
        var post = new PostUpdateDto();

        var response = await Request(HttpMethod.Post, "/api/posts", post);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);

        var responseContent = await response.Content.ReadAsStringAsync();

        var problemDetails = JsonHelper.Deserialize<ValidationProblemDetails>(responseContent);
        problemDetails!.Status.Should().Be(422);
        problemDetails!.Title.Should().NotBe(string.Empty);
        problemDetails!.Errors.Count.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Http500ErrorTest()
    {
        var response = await Request(HttpMethod.Post, "/api/statistics", null);

        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);

        var responseContent = await response.Content.ReadAsStringAsync();

        var problemDetails = JsonHelper.Deserialize<ProblemDetails>(responseContent);
        problemDetails!.Status.Should().Be(500);
        problemDetails!.Title.Should().NotBe(string.Empty);
    }
}