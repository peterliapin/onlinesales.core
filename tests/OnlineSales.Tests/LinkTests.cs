// <copyright file="LinkTests.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Nest;

namespace OnlineSales.Tests;

public class LinkTests : BaseTest
{
    [Fact]
    public async Task CreateAndFollowLinkTest()
    {
        var link = new TestLink();

        var location = await PostTest("/api/links", link);

        location.Should().NotBeNull();

        var response = await GetTest("/l/" + link.Uid, HttpStatusCode.TemporaryRedirect, "Anonymous");

        var destination = response.Headers?.Location?.AbsoluteUri ?? string.Empty;

        destination.Should().Be(link.Destination);
        var dbContext = App.GetDbContext() !;

        var linkLog = dbContext!.LinkLogs!.FirstOrDefault();

        linkLog.Should().NotBeNull();
        linkLog!.Destination.Should().Be(link.Destination);
    }
}

