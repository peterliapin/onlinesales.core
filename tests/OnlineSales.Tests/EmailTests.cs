// <copyright file="EmailTests.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

namespace OnlineSales.Tests;

public class EmailTests : BaseTest
{
    [Fact]
    public async Task RetrieveInformationsFromEmail()
    {
        var email = "hello@wave-access.com";
        var url = "/api/email/verify/" + email;
        var requestedData = await this.GetTest(url);

        requestedData.Should().NotBeNull();
    }
}
