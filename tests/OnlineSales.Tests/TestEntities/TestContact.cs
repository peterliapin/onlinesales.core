// <copyright file="TestContact.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

namespace OnlineSales.Tests.TestEntities;

public class TestContact : ContactCreateDto
{
    public TestContact(string uid = "")
    {
        this.Email = $"contact{uid}@test{uid}.net";
    }
}