// <copyright file="TestLink.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

namespace OnlineSales.Tests.TestEntities;

public class TestLink : LinkCreateDto
{
    public TestLink()
    {
        Uid = "google_link";
        Destination = "https://google.com/";
        Name = "Google Link";
    }
}

