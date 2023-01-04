// <copyright file="EmailGroupsTests.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using FluentAssertions;
using OnlineSales.DTOs;
using OnlineSales.Entities;
using OnlineSales.Tests.TestEntities.BulkPopulate;

namespace OnlineSales.Tests;
public class EmailGroupsTests : SimpleTableTests<EmailGroup, TestEmailGroup, EmailGroupUpdateDto, TestBulkEmailGroups>
{
    public EmailGroupsTests()
        : base("/api/email-groups")
    {
    }

    protected override EmailGroupUpdateDto UpdateItem(TestEmailGroup to)
    {
        var from = new EmailGroupUpdateDto();
        to.Name = from.Name = to.Name + "Updated";
        return from;
    }
}