// <copyright file="TestOrder.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using OnlineSales.DTOs;
using OnlineSales.Entities;

namespace OnlineSales.Tests.TestEntities;

public class TestOrder : OrderCreateDto
{
    public TestOrder()
    {
        RefNo = "1000";
    }
}