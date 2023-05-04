// <copyright file="TestEmailGroup.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

namespace OnlineSales.Tests.TestEntities;

public class TestEmailGroup : EmailGroupCreateDto
{
    public TestEmailGroup(string uid = "")
    {
        this.Name = $"TestEmailGroup{uid}";
    }
}