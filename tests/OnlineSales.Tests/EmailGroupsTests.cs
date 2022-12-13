// <copyright file="EmailGroupsTests.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using FluentAssertions;
using OnlineSales.DTOs;
using OnlineSales.Entities;

namespace OnlineSales.Tests;
public class EmailGroupsTests : SimpleTableTests<EmailGroup, TestEmailGroupCreate, TestEmailGroupUpdate>
{
    public EmailGroupsTests()
        : base("/api/email-groups")
    {
    }

    protected override TestEmailGroupUpdate UpdateItem(TestEmailGroupCreate to)
    {
        var from = new TestEmailGroupUpdate();
        to.Name = from.Name ?? to.Name;
        return from;
    }
}