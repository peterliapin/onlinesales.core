// <copyright file="TestLink.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

namespace OnlineSales.Tests.TestEntities;

public class TestLink : LinkCreateDto
{
    public TestLink()
    {
        this.Uid = "google_link";
        this.Destination = "https://google.com/";
        this.Name = "Google Link";
    }
}

