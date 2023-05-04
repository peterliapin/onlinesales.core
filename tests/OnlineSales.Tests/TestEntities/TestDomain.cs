// <copyright file="TestDomain.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

namespace OnlineSales.Tests.TestEntities;

public class TestDomain : DomainCreateDto
{
    public TestDomain(string name = "")
    {
        this.Name = $"gmail{name}.com";
    }
}