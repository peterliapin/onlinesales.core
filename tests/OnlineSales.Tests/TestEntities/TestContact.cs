// <copyright file="TestContact.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using OnlineSales.DTOs;

namespace OnlineSales.Tests.TestEntities;

public class TestContact : ContactCreateWithDomainDto
{
    public TestContact(string uid = "")
    {
        Email = $"contact{uid}@test{uid}.net";
    }
}