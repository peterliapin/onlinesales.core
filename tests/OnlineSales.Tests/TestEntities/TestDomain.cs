// <copyright file="TestDomain.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using OnlineSales.DTOs;

namespace OnlineSales.Tests.TestEntities;

public class TestDomain : DomainCreateDto
{
    public TestDomain(string name = "")
    {
        Name = $"gmail{name}.com";
    }
}