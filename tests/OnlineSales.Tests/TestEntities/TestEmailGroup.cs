// <copyright file="TestEmailGroup.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System;
using Nest;
using OnlineSales.DTOs;

namespace OnlineSales.Tests.TestEntities;

public class TestEmailGroupCreate : EmailGroupCreateDto
{
    public TestEmailGroupCreate()
    {
        Name = "TestEmailGroup";
    }
}

public class TestEmailGroupUpdate : EmailGroupUpdateDto
{
    public TestEmailGroupUpdate()
    {
        Name = "TestEmailGroupUpdate";
    }
}

public class TestEmailGroupConverter : ITestTypeConverter<TestEmailGroupUpdate, TestEmailGroupCreate>
{
    public void Convert(TestEmailGroupUpdate from, TestEmailGroupCreate to)
    {
        to.Name = from.Name ?? to.Name;
    }
}
